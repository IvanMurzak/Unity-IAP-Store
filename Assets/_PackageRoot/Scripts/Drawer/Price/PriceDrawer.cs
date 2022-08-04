using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;

namespace Project.Store
{
    public abstract class PriceDrawer : SerializedMonoBehaviour
    {
        protected bool      ValidateID      ()          => StoreSO.Instance != null && StoreSO.Instance.ValidateID(productID);
        protected bool      ValidateIDStr   (string x)  => StoreSO.Instance?.ValidateID(x) ?? false;     
        protected string[]  AllSellableIDs  ()          => StoreSO.Instance?.AllSellableIDs() ?? new string[] { };

        [ValidateInput("ValidateIDStr")]
        [ValueDropdown("AllSellableIDs", IsUniqueList = true)]
        [SerializeField, Required]  string  productID;
        [SerializeField]            bool    refreshOnStart  = false;
        [SerializeField]            bool    refreshOnEnable = true;

        void OnEnable()
        {
            if (refreshOnEnable) RefreshPrice();
        }
        void Awake()
        {
            StoreSO.Instance.OnInitialized
                .Subscribe  (iapInitializer => RefreshPrice())
                .AddTo      (this);
        }
        void Start()
        {
            if (refreshOnStart) RefreshPrice();
        }
        [Button(ButtonSizes.Medium)]
        public void RefreshPrice()
        {
            var sellable = StoreSO.Instance.GetSellable(productID);
            if (sellable == null)
            {
                Debug.LogError($"Sellable with id={productID}not found in Store.");
                return;
            }
            var priceString = StoreSO.Instance.GetIAPPriceString(sellable);

            SetPriceText(priceString);
        }
        protected abstract void SetPriceText(string priceString);
    }
}