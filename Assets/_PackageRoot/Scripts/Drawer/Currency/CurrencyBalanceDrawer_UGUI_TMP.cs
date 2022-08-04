using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Project.Store
{
    public class CurrencyBalanceDrawer_UGUI_TMP : CurrencyBalanceDrawer
    {
        [Required, SerializeField] TextMeshProUGUI  text;
        [Required, SerializeField] Image            icon;

        public override void RefreshIcon() => icon.sprite = StoreSO.Instance.GetCurrencyIcon(currency);

        protected override void SetPriceText(string priceString) => text.SetText(priceString);
    }
}