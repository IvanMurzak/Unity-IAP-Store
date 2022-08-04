using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using BigInt = System.Numerics.BigInteger;

namespace Project.Store
{
    public abstract class CurrencyBalanceDrawer : SerializedMonoBehaviour
    {
        protected bool      ValidateCurrency    ()          => StoreSO.Instance != null && StoreSO.Instance.ValidateCurrency(currency);
        protected bool      ValidateCurrencyStr (string x)  => StoreSO.Instance?.ValidateCurrency(x) ?? false;     
        protected string[]  AllCurrencies       ()          => StoreSO.Instance?.AllCurrenciesNames() ?? new string[] { };

        [ValidateInput("ValidateCurrencyStr")]
        [ValueDropdown("AllCurrencies", IsUniqueList = true)]
        [SerializeField, Required]  protected string  currency;
        [SerializeField]            protected bool    refreshOnStart  = false;
        [SerializeField]            protected bool    refreshOnEnable = true;

        void OnEnable()
        {
            if (refreshOnEnable) RefreshPrice();
        }
        void Awake()
        {
            StoreSO.Instance.OnInitialized
                .Subscribe  (iapInitializer =>
                {
                    RefreshIcon();
                    RefreshPrice();
                })
                .AddTo      (this);

            StoreSO.Instance.OnBalanceChanged(currency)
                .Subscribe  (balance => RefreshPrice())
                .AddTo      (this);
        }
        void Start()
        {
            if (refreshOnStart) RefreshPrice();
        }
        [Button(ButtonSizes.Medium)]
        public void RefreshPrice()
        {
            var balance = StoreSO.Instance.GetBalance(currency);
            SetPriceText(Format(balance));
        }
        public abstract void RefreshIcon();
        protected virtual string Format(BigInt balance) => $"{balance}";
        protected abstract void SetPriceText(string priceString);
    }
}