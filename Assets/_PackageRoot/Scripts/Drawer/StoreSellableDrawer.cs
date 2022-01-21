using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UniRx;

namespace Project.Store
{ 
    public abstract class StoreSellableDrawer : SerializedMonoBehaviour
    {
        protected bool      ValidateID      ()          => StoreSO.Instance != null && StoreSO.Instance.ValidateID(ID);
        protected bool      ValidateIDStr   (string x)  => StoreSO.Instance?.ValidateID(x) ?? false;      
        protected string[]  AllSellableIDs  ()          => StoreSO.Instance?.AllSellableIDs() ?? new string[] { };

        [ValidateInput("ValidateIDStr")]
        [ValueDropdown("AllSellableIDs", IsUniqueList = true)]
        [OnValueChanged("UpdateDrawer")]
        public                  string          ID;

        public                  StoreSellable   Sellable            => StoreSO.Instance == null ? null : StoreSO.Instance.GetSellable(ID);

        public      abstract    void            SetTitle            (string title);
        public      abstract    void            SetQuantity         (string quantity);
        public      abstract    void            SetPrice            (int index, Price price);
        public      abstract    void            SetIAPPrice         (string price);
        public      abstract    void            SetCurrencyIcon     (int index, Sprite sprite);
        public      abstract    void            SetSellableIcon     (Sprite sprite);
    
        public      abstract    void            SetSubActive        (int index, bool active);
        public      abstract    void            SetSubTitle         (int index, string title);
        public      abstract    void            SetSubQuantity      (int index, string quantity);
        public      abstract    void            SetSubSellableIcon  (int index, Sprite sprite);

        protected   abstract    void            OnEnoughResource    (int index, Price price, bool isEnough);

        protected   virtual     string          GetTitleText        (IStoreSellable sellable) => sellable.Title;
        protected   virtual     string          GetQuantity         (IStoreSellable sellable) => sellable.Quantity.ToString();
        protected   virtual     Sprite          GetSellableSprite   (IStoreSellable sellable) => sellable.Sprite;

        protected   virtual     void            Awake               ()
        {
            StoreSO.Instance.OnInitialized
                .Subscribe(iapInitializer => UpdateDrawer())
                .AddTo(this);
        }
        protected   virtual     void            Start               ()
        {
            UpdateDrawer();

            if (!Sellable.isIAP)
            {
                var onBalanceChanged = Sellable.required
                    .Select((required, index) => new
                    {
                        index,
                        onBalanceChanged = StoreSO.Instance.OnBalanceChanged(required.Currency)
                    });

                foreach (var x in onBalanceChanged)
                {
                    x.onBalanceChanged
                        .Subscribe(balance => OnBalanceChanged(x.index, balance))
                        .AddTo(this);
                }
            }
        }
        protected   virtual     void            OnBalanceChanged    (int index, Price price)
        {
            OnEnoughResource(index, price, Sellable.isIAP || StoreSO.Instance.IsEnoughBalance(price));
        }

        [Button(ButtonSizes.Medium)]
        public      virtual     void            UpdateDrawer        ()
        {
            if (StoreSO.Instance == null)
            {
                throw new Exception("StoreSO can't be null. Please create and activate StoreSO (ScriptableObject) instance. And don't forget to extend from StoreSO firstly");
            }

            SetTitle        (GetTitleText(Sellable));
            SetQuantity     (GetQuantity(Sellable));
            SetSellableIcon (GetSellableSprite(Sellable));

            if (Sellable.isIAP)
            {
    #if UNITY_EDITOR
                SetIAPPrice("$9.99");
    #else
                SetIAPPrice(StoreSO.Instance.GetIAPPriceString(Sellable));
    #endif
            }
            else
            {
                var count = Sellable.required.Count;

                for (int i = 0; i < count; i++)
                {
                    SetPrice(i, Sellable.required[i]);
                    SetCurrencyIcon(i, StoreSO.Instance.GetCurrencyIcon(Sellable.required[i].Currency));
                }
            }

            var subSellables = Sellable.SubSellables.ToList();
            for (var i = 0; i < subSellables.Count; i++)
            {
                SetSubActive        (i, true);
                SetSubTitle         (i, GetTitleText(subSellables[i]));
                SetSubQuantity      (i, GetQuantity(subSellables[i]));
                SetSubSellableIcon  (i, GetSellableSprite(subSellables[i]));
            }
        }
        public      virtual     void            Purchase            ()
        {
            StoreSO.Instance.Purchase(Sellable);
        }
    }
}