using System;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Project.Store
{
	public partial class UnityIAPInitializer
    {
        #region Events
        Subject<Product> onPromotionalPurchaseInterceptor = new Subject<Product>();
        public IObservable<Product> OnPromotionalPurchaseInterceptor => onPromotionalPurchaseInterceptor == null ? onPromotionalPurchaseInterceptor = new Subject<Product>() : onPromotionalPurchaseInterceptor;
        #endregion

        protected virtual ConfigurationBuilder ConfigureBuilderApple(ConfigurationBuilder builder)
        {
            // On iOS and tvOS we can intercept promotional purchases that come directly from the App Store.
            // On other platforms this will have no effect; OnPromotionalPurchase will never be called.
            builder.Configure<IAppleConfiguration>()
                .SetApplePromotionalPurchaseInterceptorCallback(OnPromotionalPurchase);

            return builder;
        }

        protected virtual void OnPromotionalPurchase(Product i)
		{
            if (debug) Debug.Log($"UnityIAPInitializer.OnPromotionalPurchase purchase id={i.definition.id}");
			onPromotionalPurchaseInterceptor.OnNext(i);
		}

		public virtual void ContinuePromotionalPurchases()
		{
            if (debug) Debug.Log($"UnityIAPInitializer.ContinuePromotionalPurchases");
			extensionsApple?.ContinuePromotionalPurchases();
		}
	}
}