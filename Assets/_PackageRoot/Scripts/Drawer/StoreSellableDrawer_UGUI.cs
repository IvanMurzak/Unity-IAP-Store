using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using System.Linq;
using DG.Tweening;

namespace Project.Store
{
    public class StoreSellableDrawer_UGUI : StoreSellableDrawer
    {
        protected bool ValidateUIPrices (List<UIPrice> list)        => Sellable == null || list == null ? false : Sellable.required.Count == list.Count || list.Count == 1 && Sellable.isIAP;
        protected bool IsNotNull        (List<UIPrice> list)        => list.All(x => x != null);
        protected bool LengthValidation (List<UISubSellable> list)  => Sellable == null ? false : list == null || Sellable.SubSellables.Count() <= list.Count;
        protected bool HasSubSellables                              => (Sellable?.SubSellables?.Count() ?? 0) > 0;

        [ValidateInput("ValidateUIPrices", "Should be similar count of resources and count of uiPrices")]
        [ValidateInput("IsNotNull", "Elements can't be null")]
        [Required,  FoldoutGroup("Settings"), ShowIf("ValidateID"), HideReferenceObjectPicker]              public          List<UIPrice>       uiPrices = new List<UIPrice>();
    
        [ShowIf("HasSubSellables")]
        [ValidateInput("LengthValidation", "Should be the same or more count to sub sellables")]
        [Required,  FoldoutGroup("Settings"), HideReferenceObjectPicker]                                    public          List<UISubSellable> subSellables = new List<UISubSellable>();

        [Required,  FoldoutGroup("Settings"), ShowIf("ValidateID")]                                         public          Image               sellableIcon;
        [           FoldoutGroup("Settings"), ShowIf("ValidateID")]                                         public          TextMeshProUGUI     textTitle;
        [           FoldoutGroup("Settings"), ShowIf("ValidateID")]                                         public          TextMeshProUGUI     textQuantity;
        [Required,  FoldoutGroup("Settings"), ShowIf("ValidateID")]                                         public          Button              button;
    
        public      override void   SetTitle                (string title)
        {
            if (textTitle != null)
            {
                textTitle.SetText(title);
                LayoutRebuilder.MarkLayoutForRebuild(textTitle.rectTransform);
            }
        }
        public      override void   SetQuantity             (string quantity)
        {
            if (textQuantity != null)
            {
                textQuantity.SetText(quantity);
                LayoutRebuilder.MarkLayoutForRebuild(textQuantity.rectTransform);
            }
        }
        public      override void   SetSellableIcon         (Sprite sprite)
        {
            if (sellableIcon != null) sellableIcon.sprite = sprite;
        }
        public      override void   SetIAPPrice             (string price)
        {
            if (string.IsNullOrEmpty(price)) price = "";
            if (uiPrices.Count > 0 && uiPrices[0]?.textPrice != null)
            {
                uiPrices[0].textPrice.SetText(price);
                LayoutRebuilder.MarkLayoutForRebuild(uiPrices[0].textPrice.rectTransform);
            }
        }
        public      override void   SetPrice                (int index, Price price)
        {
            if (uiPrices[index].textPrice != null)
            {
                uiPrices[index].textPrice.text = $"{price.Amount}";
            }
        }
        public      override void   SetCurrencyIcon         (int index, Sprite sprite)
        {
            if (uiPrices.Count > index && uiPrices[index].iconCurrency != null)
            {
                uiPrices[index].iconCurrency.gameObject.SetActive(sprite != null);
                uiPrices[index].iconCurrency.sprite = sprite;
            }
        }

    
        public      override void   SetSubActive            (int index, bool active)            
        {
            var count = (Sellable?.SubSellables?.Count() ?? 0);
            if (subSellables != null && subSellables.Count > index && count > index)
            {
                subSellables[index].root.SetActive(active);
            }
        }
        public      override void   SetSubTitle             (int index, string title)           
        {
            if (subSellables[index].textTitle)
            {
                if (subSellables[index].textTitle != null)
                {
                    subSellables[index].textTitle.text = title;
                    LayoutRebuilder.MarkLayoutForRebuild(subSellables[index].textTitle.rectTransform);
                }
            }
        }
        public      override void   SetSubQuantity          (int index, string quantity)        
        {
            if (subSellables[index].textQuantity)
            {
                if (subSellables[index].textQuantity != null)
                {
                    subSellables[index].textQuantity.text = quantity;
                    LayoutRebuilder.MarkLayoutForRebuild(subSellables[index].textQuantity.rectTransform);
                }
            }
        }
        public      override void   SetSubSellableIcon      (int index, Sprite sprite)          
        { 
            if (subSellables.Count > index && subSellables[index].sellableIcon != null) 
            {
                if (subSellables[index].sellableIcon != null)
                {
                    subSellables[index].sellableIcon.sprite = sprite;
                    subSellables[index].sellableIcon.gameObject.SetActive(sprite != null);
                }
            }
        }

        public      override void   Purchase                ()
        {
            base.Purchase();
        }
        public      override void   UpdateDrawer            ()
        {
            base.UpdateDrawer();
            if (subSellables != null && Sellable.SubSellables != null)
            {
                for (int i = Sellable.SubSellables.Count(); i < subSellables.Count; i++)
                {
                    SetSubActive(i, false);
                }
            }
        }

        protected   override void   Start                   ()
        {
            base.Start();

            uiPrices.ForEach(x =>
            {
                x.textPriceOriginalColor = x.textPrice.color;
                x.textPriceOriginalScale = x.textPrice.transform.localScale;
            });

            button.OnClickAsObservable()
                .Subscribe  (x =>
                {
                    Purchase();

                    var sellable = Sellable;
                    if (!StoreSO.Instance.IsEnoughBalance(sellable))
                    {
                        for (int i = 0; i < sellable.required.Count; i++)
                        {
                            var index = i;
                            if (!StoreSO.Instance.IsEnoughBalance(sellable.required[index]))
						    {
                                DOTween.Kill(uiPrices[index].textPrice.GetInstanceID());
                                DOTween.Kill(uiPrices[index].textPrice.transform.GetInstanceID());

                                uiPrices[index].textPrice.DOColor(Color.red, 0.3f, uiPrices[index].textPriceOriginalColor)
                                    .SetLoops(2, LoopType.Yoyo)
                                    .SetId(uiPrices[index].textPrice.GetInstanceID())
                                    .OnComplete(() => uiPrices[index].textPrice.color = uiPrices[index].textPriceOriginalColor);

                                uiPrices[index].textPrice.transform.DOScale(1.2f, 0.3f)
                                    .From(uiPrices[index].textPriceOriginalScale)
                                    .SetLoops(2, LoopType.Yoyo)
                                    .SetId(uiPrices[index].textPrice.transform.GetInstanceID())
                                    .OnComplete(() => uiPrices[index].textPrice.transform.localScale = uiPrices[index].textPriceOriginalScale);
                            }
                        }
                    }
                })
                .AddTo      (this);
        }
        protected   override void   OnEnoughResource        (int index, Price price, bool isEnough)
        {
            // uiPrices[index].textPrice.SetText($"{price.amount}");
        }
    }

    public static class TextMeshProUGUIDOTWeenStoreHelpers
    {
        public static Tween DOColor(this TextMeshProUGUI tmp, Color to, float duration, Color from) => DOTween.To(() => tmp.color, x => tmp.color = x, to, duration).From(from);
    }
}