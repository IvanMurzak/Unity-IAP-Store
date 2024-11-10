using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Project.Store
{
    public partial class UnityIAPInitializer : IStoreListener
    {
        public const string ON_PRODUCT_PURCHASED = "ON_PRODUCT_PURCHASED";
        public const string ON_PRODUCT_PURCHASING_FAILED = "ON_PRODUCT_PURCHASING_FAILED";

        #region Events
        Subject<UnityIAPInitializer> onInitializedIAP = new Subject<UnityIAPInitializer>();
        public IObservable<UnityIAPInitializer> OnInitializedIAP => onInitializedIAP == null ? onInitializedIAP = new Subject<UnityIAPInitializer>() : onInitializedIAP;

        Subject<TransactionContainer> onProductPurchased = new Subject<TransactionContainer>();
        public IObservable<TransactionContainer> OnProductPurchased => onProductPurchased == null ? onProductPurchased = new Subject<TransactionContainer>() : onProductPurchased;

        Subject<TransactionContainer> onProductPurchasingFailed = new Subject<TransactionContainer>();
        public IObservable<TransactionContainer> OnProductPurchasingFailed => onProductPurchasingFailed == null ? onProductPurchasingFailed = new Subject<TransactionContainer>() : onProductPurchasingFailed;

        Subject<bool> onRestorePurchasesCompleted = new Subject<bool>();
        public IObservable<bool> OnRestorePurchasesCompleted => onRestorePurchasesCompleted == null ? onRestorePurchasesCompleted = new Subject<bool>() : onRestorePurchasesCompleted;
        #endregion

        public bool useFakeStore;
        [ShowIf("useFakeStore")]
        public FakeStoreUIMode fakeStoreUIMode;

        public Product Product(string id, bool printError = false)
        {
            if (controller == null)
            {
                if (!Application.isEditor && !useFakeStore)
                    Debug.LogError("UnityIAPInitializer.Product Controller still had not been initialized");
                return null;
            }
            var product = controller.products.WithID(id);
            if (product == null && printError)
                Debug.LogError($"UnityIAPInitializer.Product Product with id='{id}' not found");
            return product;
        }
        public bool HasProduct(string id, bool printError = false) => Product(id, printError) != null;
        public bool IsSubscribed(string id) => IsSubscribed(Product(id, true));
        public bool IsSubscribed(Product product)
        {
            if (CheckIfProductIsAvailableForSubscriptionManager(product))
            {
                var meta = product.metadata.GetGoogleProductMetadata();
                var subscriptionManager = new SubscriptionManager(product, meta?.originalJson);
                var subscriptionInfo = subscriptionManager.getSubscriptionInfo();

                return subscriptionInfo.isSubscribed() == Result.True;
            }
            return false;
        }
        public bool IsOwnedNonConsumable(string id) => IsOwnedNonConsumable(Product(id, true));
        public bool IsOwnedNonConsumable(Product product)
        {
            if (product?.definition == null) return false;
            if (product.definition.type == ProductType.NonConsumable)
                return product.hasReceipt;
            return false;
        }

        protected IStoreController controller;

        protected IExtensionProvider extensions;
        protected IAppleExtensions extensionsApple;
        protected IMicrosoftExtensions extensionsMicrosoft;
        protected IUDPExtensions extensionsUDP;
        protected IAmazonExtensions extensionsAmazon;
        protected ITransactionHistoryExtensions extensionsTransactionHistory;
        protected IGooglePlayStoreExtensions extensionsGooglePlayStore;

        private bool isInited;
        protected bool debug;

        public virtual void InitiatePurchase(string productId)
        {
            if (controller == null)
            {
                if (!Application.isEditor && !useFakeStore)
                    Debug.LogError("UnityIAPInitializer.Controller still had not been initialized");
                return;
            }
            controller.InitiatePurchase(productId);
        }
        public virtual void RestorePurchases()
        {
            if (debug) Debug.Log($"UnityIAPInitializer.RestorePurchases");

            var nonConsumableProducts = controller.products.all
                .Where(product => product.definition.type == ProductType.NonConsumable)
                .Where(product => !product.hasReceipt)
                .ToDictionary(product => product.definition.id);

            extensionsApple?.RestoreTransactions(success =>
            {
                if (debug) Debug.Log($"UnityIAPInitializer.RestorePurchases Restore Transactions completed with status:{(success ? "success" : "failed")}");
                if (success) InvalidateRestoredProducts(nonConsumableProducts);
                onRestorePurchasesCompleted.OnNext(success);
            });
            extensionsGooglePlayStore?.RestoreTransactions(success =>
            {
                if (debug) Debug.Log($"UnityIAPInitializer.RestorePurchases Restore Transactions completed with status:{(success ? "success" : "failed")}");
                if (success) InvalidateRestoredProducts(nonConsumableProducts);
                onRestorePurchasesCompleted.OnNext(success);
            });
            extensionsMicrosoft?.RestoreTransactions();
            // extensionsUDP does not exist in UDP API
            // extensionsAmazon not supported by Amazon
        }
        protected virtual void InvalidateRestoredProducts(Dictionary<string, Product> nonConsumableProducts)
        {
            var newNonConsumableProducts = controller.products.all
                .Where(product => product.definition.type == ProductType.NonConsumable)
                .Where(product => product.hasReceipt)
                .ToList();

            foreach (var product in newNonConsumableProducts)
            {
                if (nonConsumableProducts.ContainsKey(product.definition.id))
                {
                    onProductPurchased.OnNext(new TransactionContainer()
                    {
                        productId = product.definition.id,
                        transactionId = product.transactionID
                    });
                }
            }
        }

        protected virtual IEnumerable<Func<PurchaseEventArgs, PurchaseProcessingResult>> PurchaseProcessors => new Func<PurchaseEventArgs, PurchaseProcessingResult>[]
        {
            purchaseEventArgs =>
            {
                return PurchaseProcessingResult.Complete;
            }
        };
        protected virtual StandardPurchasingModule CreatePurchasingModule()
        {
            var instance = StandardPurchasingModule.Instance();
            instance.useFakeStoreAlways = useFakeStore;
            instance.useFakeStoreUIMode = fakeStoreUIMode;
            return instance;
        }
        protected virtual ConfigurationBuilder ConfigureBuilder(ConfigurationBuilder builder)
        {
            if (debug) Debug.Log($"UnityIAPInitializer.ConfigureBuilder");
            builder = ConfigureBuilderAndroid(builder);
            builder = ConfigureBuilderApple(builder);

            return builder;
        }

        public virtual void Init(IEnumerable<StoreSellable> iapSellables, bool debug = true)
        {
            this.debug = debug;
            if (debug) Debug.Log($"UnityIAPInitializer.Init");

            if (isInited && controller != null) return;
            var productDefinitions = iapSellables.Select(x =>
            {
                var definition = new StoreProductDefinition(x.ID, x.IAP_StoreSpecitifID, x.IAP_ProductType, true, x.IAP_PayoutDefinition, x.applePromotionVisibility);
                return definition;
            });
            if (debug)
            {
                foreach (var definition in productDefinitions)
                    Debug.Log($"UnityIAPInitializer.Init id={definition.id}, storeSpecificId={definition.storeSpecificId}, productType={definition.type}, payout={definition.payout}");
            }
            var builder = ConfigurationBuilder.Instance(CreatePurchasingModule());
            builder = builder.AddProducts(productDefinitions);
            builder = ConfigureBuilder(builder);

            UnityPurchasing.Initialize(this, builder);
            isInited = true;
        }
        public virtual void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.controller = controller;
            this.extensions = extensions;

            extensionsApple = extensions.GetExtension<IAppleExtensions>();
            extensionsMicrosoft = extensions.GetExtension<IMicrosoftExtensions>();
            extensionsUDP = extensions.GetExtension<IUDPExtensions>();
            extensionsAmazon = extensions.GetExtension<IAmazonExtensions>();
            extensionsTransactionHistory = extensions.GetExtension<ITransactionHistoryExtensions>();
            extensionsGooglePlayStore = extensions.GetExtension<IGooglePlayStoreExtensions>();

            extensionsApple.RegisterPurchaseDeferredListener(async product =>
            {
                await UniTask.DelayFrame(1);
                onProductPurchased.OnNext(new TransactionContainer() { productId = product.definition.id, transactionId = product.transactionID });
            });

            // Set all these products to be visible in the user's App Store
            foreach (var item in controller.products.all)
            {
                if (item.availableToPurchase)
                    extensionsApple?.SetStorePromotionVisibility(item, ((StoreProductDefinition)item.definition).applePromotionVisibility);
            }

            if (debug) Debug.Log("UnityIAPInitializer.OnInitialized Unity IAP Initialized");
            onInitializedIAP.OnNext(this);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"UnityIAPInitializer.OnInitializeFailed error='{error}'");
        }
        public virtual void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"UnityIAPInitializer.OnInitializeFailed message='{message}', error='{error}'");
        }
        public virtual PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            var transaction = new TransactionContainer()
            {
                productId = e.purchasedProduct.definition.id,
                transactionId = e.purchasedProduct.transactionID
            };
            foreach (var purchaseProcessor in PurchaseProcessors)
            {
                var result = purchaseProcessor(e);
                if (result == PurchaseProcessingResult.Complete)
                {
                    onProductPurchased.OnNext(transaction);
                    return PurchaseProcessingResult.Complete;
                }
            }
            Debug.LogError($"UnityIAPInitializer.ProcessPurchase No registered PurchaseProcessor for productId={e.purchasedProduct.definition.id}");
            onProductPurchasingFailed.OnNext(transaction);
            return PurchaseProcessingResult.Pending;
        }
        public virtual void OnPurchaseFailed(Product i, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"UnityIAPInitializer.OnPurchaseFailed purchase id={i.definition.id}");
            onProductPurchasingFailed.OnNext(new TransactionContainer()
            {
                productId = i.definition.id,
                transactionId = i.transactionID,
                failureReason = failureReason
            });
        }
    }
}
