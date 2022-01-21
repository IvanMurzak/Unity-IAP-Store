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
		public	const			string															ON_PRODUCT_PURCHASED			= "ON_PRODUCT_PURCHASED";
		public	const			string															ON_PRODUCT_PURCHASING_FAILED	= "ON_PRODUCT_PURCHASING_FAILED";

		[NonSerialized]			Subject<UnityIAPInitializer>									onInitializedIAP				= new Subject<UnityIAPInitializer>();
		public					IObservable<UnityIAPInitializer>								OnInitializedIAP				=> onInitializedIAP;

		[NonSerialized]			Subject<TransactionContainer>									onProductPurchased				= new Subject<TransactionContainer>();
		public					IObservable<TransactionContainer>								OnProductPurchased				=> onProductPurchased == null ? onProductPurchased = new Subject<TransactionContainer>() : onProductPurchased;

		[NonSerialized]			Subject<TransactionContainer>									onProductPurchasingFailed		= new Subject<TransactionContainer>();
		public					IObservable<TransactionContainer>								OnProductPurchasingFailed		=> onProductPurchasingFailed == null ? onProductPurchasingFailed = new Subject<TransactionContainer>() : onProductPurchasingFailed;
	
		public					bool															useFakeStore;
		[ShowIf("useFakeStore")]
		public					FakeStoreUIMode													fakeStoreUIMode;

		public					Product															Product							(string id, bool printError = false)
		{
			if (controller == null)
            {
				if (!Application.isEditor && !useFakeStore) Debug.LogError("Controller still had not been initialized");
				return null;
            }
			var product = controller.products.WithID(id);
			if (product == null && printError)
				Debug.LogError($"Product with id='{id}' didn't found");
			return product;
		}
		public					bool															HasProduct						(string id, bool printError = false)	=> Product(id, printError) != null;
		public					bool															IsSubscribed					(string id)								=> IsSubscribed(Product(id, true));
		public					bool															IsSubscribed					(Product product)
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
		public					bool															IsOwnedNonConsumable			(string id)								=> IsOwnedNonConsumable(Product(id, true));
		public					bool															IsOwnedNonConsumable			(Product product)
		{
			if (product?.definition == null) return false;
			if (product.definition.type == ProductType.NonConsumable)
				return product.hasReceipt;
			return false;
		}

		private					IStoreController												controller;

		private					IExtensionProvider												extensions;
		private					IAppleExtensions												extensionsApple;
		private					IMicrosoftExtensions											extensionsMicrosoft;
		private					IUDPExtensions													extensionsUDP;
		private					IAmazonExtensions												extensionsAmazon;
		private					ITransactionHistoryExtensions									extensionsTransactionHistory;
		private					IGooglePlayStoreExtensions										extensionsGooglePlayStore;

		private					bool															isInited;

		public		virtual		void															InitiatePurchase				(string productId)
		{
			if (controller == null)
			{
				if (!Application.isEditor && !useFakeStore) Debug.LogError("Controller still had not been initialized");
				return;
			}
			controller.InitiatePurchase(productId);
		}
		public		virtual		void															RestorePurchases				()
        {
			Debug.Log($"RestorePurchases");
			extensionsApple?.RestoreTransactions(success =>
			{
				Debug.Log($"Restore Transactions completed with status:{(success ? "success" : "failed")}");
			});
			extensionsGooglePlayStore?.RestoreTransactions(success =>
			{
				Debug.Log($"Restore Transactions completed with status:{(success ? "success" : "failed")}");
			});
			extensionsMicrosoft?.RestoreTransactions();
			// extensionsUDP does not exist in UDP API
			// extensionsAmazon not supported by Amazon
		}

		protected	virtual		IEnumerable<Func<PurchaseEventArgs, PurchaseProcessingResult>>	PurchaseProcessors				=> new Func<PurchaseEventArgs, PurchaseProcessingResult>[] 
		{ 
			purchaseEventArgs =>
			{
				return PurchaseProcessingResult.Complete;
			}
		};
		protected	virtual		StandardPurchasingModule										CreatePurchasingModule()
		{
			var instance = StandardPurchasingModule.Instance();
				instance.useFakeStoreAlways = useFakeStore;
				instance.useFakeStoreUIMode = fakeStoreUIMode;
			return instance;
		}

		public		virtual		void															Init							(IEnumerable<StoreSellable> iapSellables)
		{
			if (isInited && controller != null) return;
			var productDefinitions = iapSellables.Select(x =>
			{
				var definition = new ProductDefinition(x.ID, x.IAP_StoreSpecitifID, x.IAP_ProductType, true, x.IAP_PayoutDefinition);
				return definition;
			});
			foreach (var definition in productDefinitions)
				Debug.Log($"id={definition.id}, storeSpecificId={definition.storeSpecificId}, productType={definition.type}, payout={definition.payout}");

			var builder = ConfigurationBuilder.Instance(CreatePurchasingModule());
				builder.AddProducts(productDefinitions);
			UnityPurchasing.Initialize(this, builder);
			isInited = true;
		}
		public		virtual		void															OnInitialized					(IStoreController controller, IExtensionProvider extensions)
		{
			this.controller					= controller;
			this.extensions					= extensions;

			extensionsApple					= extensions.GetExtension<IAppleExtensions>();
			extensionsMicrosoft				= extensions.GetExtension<IMicrosoftExtensions>();
			extensionsUDP					= extensions.GetExtension<IUDPExtensions>();
			extensionsAmazon				= extensions.GetExtension<IAmazonExtensions>();
			extensionsTransactionHistory	= extensions.GetExtension<ITransactionHistoryExtensions>();
			extensionsGooglePlayStore		= extensions.GetExtension<IGooglePlayStoreExtensions>();

			extensionsApple.RegisterPurchaseDeferredListener(async product =>
			{
				await UniTask.DelayFrame(1);
				onProductPurchased.OnNext(new TransactionContainer() { productId = product.definition.id, transactionId = product.transactionID });
			});

			Debug.Log("StoreSO: Unity IAP Initialized");
			onInitializedIAP.OnNext(this);
		}
		public		virtual		void															OnInitializeFailed				(InitializationFailureReason error)
		{
			Debug.LogError($"IAP initialization error={error}");
		}
		public		virtual		PurchaseProcessingResult										ProcessPurchase					(PurchaseEventArgs e)
		{
			var transaction = new TransactionContainer() { productId = e.purchasedProduct.definition.id, transactionId = e.purchasedProduct.transactionID };
			foreach (var purchaseProcessor in PurchaseProcessors)
			{
				var result = purchaseProcessor(e);
				if (result == PurchaseProcessingResult.Complete)
				{
					onProductPurchased.OnNext(transaction);
					return PurchaseProcessingResult.Complete;
				}
			}
			Debug.LogError($"No registered PurchaseProcessor for productId={e.purchasedProduct.definition.id}");
			onProductPurchasingFailed.OnNext(transaction);
			return PurchaseProcessingResult.Pending;
		}
		public		virtual		void															OnPurchaseFailed				(Product i, PurchaseFailureReason p)
		{
			Debug.LogError($"purchase id={i.definition.id}");
			onProductPurchasingFailed.OnNext(new TransactionContainer() { productId = i.definition.id, transactionId = i.transactionID });
		}
	}
}