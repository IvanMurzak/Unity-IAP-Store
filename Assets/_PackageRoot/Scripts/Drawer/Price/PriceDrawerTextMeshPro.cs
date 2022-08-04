using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;

namespace Project.Store
{
    public class PriceDrawerTextMeshPro : PriceDrawer
    {
        [SerializeField, Required] TextMeshProUGUI text;
        protected override void SetPriceText(string priceString)
        {
            if (text != null) text.text = priceString;
        }
    }
}