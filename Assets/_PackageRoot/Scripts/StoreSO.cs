using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEngine.Purchasing;
using BigInt = System.Numerics.BigInteger;

namespace Project.Store
{
    public abstract class StoreSO : SerializedScriptableObject
    {
        public static StoreSO Instance { get; private set; }

        #region Events

        private Subject<UnityIAPInitializer> onInitialized = new Subject<UnityIAPInitializer>();
        public IObservable<UnityIAPInitializer> OnInitialized => onInitialized;

        private Subject<StoreSellable> onPurchaseSuccessful = new Subject<StoreSellable>();
        public IObservable<StoreSellable> OnPurchaseSuccessful => onPurchaseSuccessful;

        private Subject<StoreSellable> onInsufficientFunds = new Subject<StoreSellable>();
        public IObservable<StoreSellable> OnInsufficientFunds => onInsufficientFunds;

        private Subject<StoreSellable> onPurchaseFailed = new Subject<StoreSellable>();
        public IObservable<StoreSellable> OnPurchaseFailed => onPurchaseFailed;

        private Subject<(StoreSellable sellable, PurchaseFailureReason reason)> onIAPPurchaseFailed = new Subject<(StoreSellable sellable, PurchaseFailureReason reason)>();
        public IObservable<(StoreSellable sellable, PurchaseFailureReason reason)> OnIAPPurchaseFailed => onIAPPurchaseFailed;

        private Subject<bool> onRestorePurchasesCompleted = new Subject<bool>();
        public IObservable<bool> OnRestorePurchasesCompleted => onRestorePurchasesCompleted;

        private Subject<Product> onPromotionalPurchaseInterceptor = new Subject<Product>();
        public IObservable<Product> OnPromotionalPurchaseInterceptor => onPromotionalPurchaseInterceptor;

        private Subject<Product> onPromotionalPurchaseContinue = new Subject<Product>();
        public IObservable<Product> OnPromotionalPurchaseContinue => onPromotionalPurchaseContinue;

        private Subject<Product> onPromotionalPurchaseCancel = new Subject<Product>();
        public IObservable<Product> OnPromotionalPurchaseCancel => onPromotionalPurchaseCancel;

        // TODO: implement Blocked event
        private Subject<StoreSellable> onPurchaseBlocked = new Subject<StoreSellable>();
        public IObservable<StoreSellable> OnPurchaseBlocked => onPurchaseBlocked;
        #endregion

        [OnValueChanged("OnEnable")] public bool isActive = true;
        public bool debug;
        [Required] public UnityIAPInitializer unityIAPInitializer;
        [OnValueChanged("InvalidateData")]
        [Required, HideReferenceObjectPicker] public List<Currency> currencies = new List<Currency>();
        [OnValueChanged("InvalidateData")]
        [Required, HideReferenceObjectPicker] public Dictionary<string, List<StoreSellable>> categories = new Dictionary<string, List<StoreSellable>>();

        Product _applePromotionalProduct = null;
        List<StoreSellable> _sellablesList;
        Dictionary<string, StoreSellable> _sellablesByIAPID;
        Dictionary<string, StoreSellable> _sellablesByID;

        List<StoreSellable> SellablesList
        {
            get
            {
                if (_sellablesList == null) InvalidateData();
                return _sellablesList;
            }
        }
        Dictionary<string, StoreSellable> SellablesByIAPID
        {
            get
            {
                if (_sellablesByIAPID == null) InvalidateData();
                return _sellablesByIAPID;
            }
        }
        Dictionary<string, StoreSellable> SellablesByID
        {
            get
            {
                if (_sellablesByID == null) InvalidateData();
                return _sellablesByID;
            }
        }

        CompositeDisposable compositeDiposable = new CompositeDisposable();

        public abstract BigInt GetBalance(string currency);
        protected abstract void SpendBalance(string currency, BigInt amount);
        public abstract IObservable<Price> OnBalanceChanged(string currency);
        protected abstract void ApplyPurchase(List<IStoreSellable> sellables);

        public bool IsEnoughBalance(Price price) => price.Amount <= GetBalance(price.Currency);
        public bool IsEnoughBalance(StoreSellable sellable) => sellable.isIAP ? true : sellable.required.All(x => IsEnoughBalance(x));
        public ReadOnlyReactiveProperty<bool> IsEnoughBalanceReactive(StoreSellable sellable) => sellable.isIAP ? new BoolReactiveProperty(true).ToReadOnlyReactiveProperty() :
            Observable.Merge(sellable.required
                    .Select(price => OnBalanceChanged(price.Currency)
                    .Select(x => IsEnoughBalance(price))))
            .ToReadOnlyReactiveProperty();

        public StoreSellable GetSellable(string id) => id == null ? null : SellablesByID.ContainsKey(id) ? SellablesByID[id] : null;
        public Sprite GetCurrencyIcon(string currency) => currencies.First(x => x.name == currency).icon;

        public string[] AllCurrenciesNames() => currencies.Select(x => x.name).ToArray();
        public string[] AllSellableIDs() => SellablesList.Select(x => x.ID).ToArray();
        public string[] AllCategories() => categories.Keys.ToArray();

        public bool ValidateID(string id) => id == null ? false : GetSellable(id) != null;
        public bool ValidateCurrency(string currency) => currency == null ? false : AllCurrenciesNames().Contains(currency);
        public bool ValidateCategory(string category) => category == null ? false : categories.ContainsKey(category);

        public bool IsProductApplePromotional => _applePromotionalProduct != null;
        public virtual void InvalidateData()
        {
            if (debug) Debug.Log("StoreSO.InvalidateData", this);
            _sellablesList = categories
                .SelectMany(x => x.Value)
                .Where(x => x != null)
                .Where(x => !string.IsNullOrEmpty(x.ID))
                .ToList();

            if (debug) Debug.Log($"StoreSO.InvalidateData invalidating {_sellablesList.Count} products", this);

            _sellablesByID = new Dictionary<string, StoreSellable>();
            _sellablesList.ForEach(x => _sellablesByID[x.ID] = x);

            _sellablesByIAPID = new Dictionary<string, StoreSellable>();
            _sellablesList.Where(x => x.isIAP && !string.IsNullOrEmpty(x.IAP_StoreSpecitifID))
                .ForEach(x => _sellablesByIAPID[x.IAP_StoreSpecitifID] = x);
        }

        protected virtual void OnEnable()
        {
            if (!isActive)
            {
                if (Instance == this)
                    Instance = null;
                return;
            }
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Multiple active StoreSO instances detected! Just one instance may have 'isActive' = 'True'", this);
                return;
            }

            Instance = this;
            if (debug) Debug.Log($"Inited StoreSO: {name}, isPlaying={Application.isPlaying}", this);

            compositeDiposable.Clear();

            InvalidateData();
            InitDebug();

            OnInsufficientFunds.Subscribe(onPurchaseFailed.OnNext).AddTo(compositeDiposable);
            OnPurchaseBlocked.Subscribe(onPurchaseFailed.OnNext).AddTo(compositeDiposable);

            OnInsufficientFunds.Subscribe(OnInsufficientFundsEvent).AddTo(compositeDiposable);
            OnPurchaseSuccessful.Subscribe(OnPurchaseSuccessfulEvent).AddTo(compositeDiposable);
            OnPurchaseFailed.Subscribe(OnPurchaseFailedEvent).AddTo(compositeDiposable);
            OnIAPPurchaseFailed.Subscribe(OnIAPPurchaseFailedEvent).AddTo(compositeDiposable);
            OnPurchaseBlocked.Subscribe(OnPurchaseBlockedEvent).AddTo(compositeDiposable);

            OnRestorePurchasesCompleted.Subscribe(OnRestorePurchasesCompletedEvent).AddTo(compositeDiposable);

            if (Application.isPlaying)
            {
                InitIAP();
            }
            else
            {
                // none
            }
        }
        protected virtual void OnDisable()
        {
            if (Instance == this)
                Instance = null;

            compositeDiposable.Clear();
        }
        private void InitIAP()
        {
            if (debug) Debug.Log($"StoreSO.InitIAP: {name}", this);
            if (SellablesByIAPID == null) Debug.LogError("SellablesByIAPID is null");
            if (unityIAPInitializer == null)
            {
                Debug.LogError("StoreSO.InitIAP unityIAPInitializer is null, replacing be default UnityIAPInitializer instance", this);
                unityIAPInitializer = new UnityIAPInitializer();
            }
            unityIAPInitializer.OnInitializedIAP
                .Subscribe(iapInitializer => onInitialized.OnNext(iapInitializer))
                .AddTo(compositeDiposable);

            unityIAPInitializer.Init(SellablesByIAPID.Values, debug);

            unityIAPInitializer.OnProductPurchased
                .Subscribe(transaction =>
                {
                    var sellable = SellablesByIAPID[transaction.productId];
                    ApplyPurchaseInternal(sellable);
                })
                .AddTo(compositeDiposable);

            unityIAPInitializer.OnProductPurchasingFailed
                .Subscribe(transaction =>
                {
                    _applePromotionalProduct = null;
                    var sellable = SellablesByIAPID[transaction.productId];
                    onPurchaseFailed.OnNext(sellable);
                    onIAPPurchaseFailed.OnNext((sellable, transaction.failureReason));
                })
                .AddTo(compositeDiposable);

            unityIAPInitializer.OnRestorePurchasesCompleted
                .Subscribe(onRestorePurchasesCompleted.OnNext)
                .AddTo(compositeDiposable);

            unityIAPInitializer.OnPromotionalPurchaseInterceptor
                .Subscribe(item =>
                {
                    // Handle this event by, e.g. presenting a parental gates.
                    if (debug) Debug.Log($"StoreSO.OnProductPromotionalPurchased: {item.definition.id}", this);
                    _applePromotionalProduct = item;
                    onPromotionalPurchaseInterceptor.OnNext(_applePromotionalProduct);
                })
                .AddTo(compositeDiposable);

            if (debug) Debug.Log($"StoreSO.InitIAP: {name}, completed", this);
        }
        private void InitDebug()
        {
            if (debug) Debug.Log($"StoreSO.InitDebug: {name}, Debug={debug}", this);

            OnPurchaseSuccessful.Where(x => debug).Subscribe(x => Debug.Log($"{name}: OnPurchaseSuccessful - {x.ID}")).AddTo(compositeDiposable);
            OnInsufficientFunds.Where(x => debug).Subscribe(x => Debug.Log($"{name}: OnInsufficientFunds - {x.ID}")).AddTo(compositeDiposable);
            OnPurchaseBlocked.Where(x => debug).Subscribe(x => Debug.Log($"{name}: OnPurchaseBlocked - {x.ID}")).AddTo(compositeDiposable);
            OnPurchaseFailed.Where(x => debug).Subscribe(x => Debug.Log($"{name}: OnPurchaseFailed - {x.ID}")).AddTo(compositeDiposable);
            OnIAPPurchaseFailed.Where(x => debug).Subscribe(x => Debug.Log($"{name}: OnIAPPurchaseFailed - {x.sellable.ID}, Reason - {x.reason}")).AddTo(compositeDiposable);
        }
        public Product GetUnityIAPProduct(StoreSellable sellable) => GetUnityIAPProduct(sellable.IAP_StoreSpecitifID);
        public Product GetUnityIAPProduct(string storeIAPProductID)
        {
            var product = unityIAPInitializer?.Product(storeIAPProductID);
            if (product == null)
            {
                if (!Application.isEditor && !(unityIAPInitializer?.useFakeStore ?? false))
                    Debug.LogError($"No registered product with IAP_ID={storeIAPProductID} found. Please add the product", this);
                return null;
            }
            return product;
        }
        public decimal GetIAPPrice(StoreSellable sellable)
        {
            var product = GetUnityIAPProduct(sellable);
            if (product == null) return -1;
            return product.metadata.localizedPrice;
        }
        public string GetIAPPriceString(StoreSellable sellable)
        {
            var product = GetUnityIAPProduct(sellable);
            if (product == null) return null;
            return product.metadata.localizedPriceString;
        }

        protected virtual void OnInsufficientFundsEvent(StoreSellable sellable) { }
        protected virtual void OnPurchaseSuccessfulEvent(StoreSellable sellable) { }
        protected virtual void OnPurchaseFailedEvent(StoreSellable sellable) { }
        protected virtual void OnIAPPurchaseFailedEvent((StoreSellable sellable, PurchaseFailureReason reason) data) { }
        protected virtual void OnRestorePurchasesCompletedEvent(bool success) { }
        protected virtual void OnPurchaseBlockedEvent(StoreSellable sellable) { }
        protected virtual void OnProductPromotionalPurchaseEvent(Product product) { }

        protected void SpendBalance(StoreSellable sellable)
        {
            foreach (var required in sellable.required)
            {
                if (debug) Debug.Log($"Store.SpendBalance, {required.Amount} of {required.Currency}", this);
                SpendBalance(required.Currency, required.Amount);
            }
        }
        private void ApplyPurchaseInternal(StoreSellable sellable)
        {
            var sellables = new List<IStoreSellable>() { sellable };
            sellables.AddRange(sellable.SubSellables);

            if (debug) sellables.ForEach(s => Debug.Log($"Store.ApplyPurchaseInternal, for ID={s.ID}, Quantity={s.Quantity}, Title={s.Title}", this));

            ApplyPurchase(sellables);

            onPurchaseSuccessful.OnNext(sellable);
        }
        public void Purchase(string id) => Purchase(GetSellable(id));
        public void Purchase(StoreSellable sellable)
        {
            if (debug) Debug.Log($"Store.Purchase, sellable.ID = {sellable.ID}, isIAP={sellable.isIAP}", this);

            // TODO: implement is Blocked check
            // if (sellable.IsBlocked)

            if (sellable.isIAP)
            {
                if (debug) Debug.Log($"Store.Purchase, sellable.IAP_StoreSpecitifID = {sellable.IAP_StoreSpecitifID}", this);
                unityIAPInitializer?.InitiatePurchase(sellable.IAP_StoreSpecitifID);
            }
            else
            {
                if (IsEnoughBalance(sellable))
                {
                    if (debug) Debug.Log($"Store.Purchase, IsEnoughBalance = true", this);
                    SpendBalance(sellable);
                    ApplyPurchaseInternal(sellable);
                }
                else
                {
                    if (debug) Debug.Log($"Store.Purchase, IsEnoughBalance = false, onInsufficientFunds emmits", this);
                    onInsufficientFunds.OnNext(sellable);
                }
            }
        }

        /// <summary>
        /// Restore all purchases made at this account. For restored purchases will be executed ApplyPurchase
        /// </summary>
        public void RestorePurchases()
        {
            if (debug) Debug.Log($"Store.RestorePurchases", this);
            unityIAPInitializer?.RestorePurchases();
        }

        /// <summary>
        /// Cancel current "iOS promotional" purchase. Does nothing at non iOS platform.
        /// Executes <see name="UnityEngine.Purchasing.IAppleExtensions.ContinuePromotionalPurchases">IAppleExtensions.ContinuePromotionalPurchases</see> from UnityIAP API.
        /// </summary>
        public void iOSPromotionalPurchaseContinue()
        {
#if !UNITY_IOS
            return;
#endif
            if (debug) Debug.Log($"Store.iOSPromotionalPurchaseContinue", this);
            if (_applePromotionalProduct == null)
            {
                if (debug) Debug.LogError($"Store.iOSPromotionalPurchaseContinue canceled, because `_applePromotionalProduct` is null", this);
                return;
            }

            onPromotionalPurchaseContinue?.OnNext(_applePromotionalProduct);
            _applePromotionalProduct = null;
            unityIAPInitializer?.ContinuePromotionalPurchases();
        }

        /// <summary>
        /// Cancel current "iOS promotional" purchase. Does nothing at non iOS platform.
        /// </summary>
        public void iOSPromotionalPurchaseCancel()
        {
#if !UNITY_IOS
            return;
#endif
            if (debug) Debug.Log($"Store.iOSPromotionalPurchaseCancel", this);
            if (_applePromotionalProduct == null)
            {
                if (debug) Debug.LogError($"Store.iOSPromotionalPurchaseCancel canceled, because `_applePromotionalProduct` is null", this);
                if (debug) Debug.LogError($"Store.iOSPromotionalPurchaseCancel canceled, because `_applePromotionalProduct` is null", this);
                return;
            }

            onPromotionalPurchaseCancel?.OnNext(_applePromotionalProduct);
            _applePromotionalProduct = null;
        }
    }
}
