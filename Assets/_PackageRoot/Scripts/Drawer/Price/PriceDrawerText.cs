using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Store
{
    public class PriceDrawerText : PriceDrawer
    {
        [SerializeField, Required] Text text;
        protected override void SetPriceText(string priceString)
        {
            if (text != null) text.text = priceString;
        }
    }
}