using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace Project.Store
{
    public class StoreProductDefinition : ProductDefinition
    {
        /// <summary>
        /// This enum is a C# representation of the Apple object `SKProductStorePromotionVisibility`
        /// </summary>
        public AppleStorePromotionVisibility applePromotionVisibility { get; private set; }

        public StoreProductDefinition(string id, ProductType type, AppleStorePromotionVisibility applePromotionVisibility = AppleStorePromotionVisibility.Default) : base(id, type)
        {
            this.applePromotionVisibility = applePromotionVisibility;
        }

        public StoreProductDefinition(string id, string storeSpecificId, ProductType type, AppleStorePromotionVisibility applePromotionVisibility = AppleStorePromotionVisibility.Default) : base(id, storeSpecificId, type)
        {
            this.applePromotionVisibility = applePromotionVisibility;
        }

        public StoreProductDefinition(string id, string storeSpecificId, ProductType type, bool enabled, AppleStorePromotionVisibility applePromotionVisibility = AppleStorePromotionVisibility.Default) : base(id, storeSpecificId, type, enabled)
        {
            this.applePromotionVisibility = applePromotionVisibility;
        }

        public StoreProductDefinition(string id, string storeSpecificId, ProductType type, bool enabled, PayoutDefinition payout, AppleStorePromotionVisibility applePromotionVisibility = AppleStorePromotionVisibility.Default) : base(id, storeSpecificId, type, enabled, payout)
        {
            this.applePromotionVisibility = applePromotionVisibility;
        }

        public StoreProductDefinition(string id, string storeSpecificId, ProductType type, bool enabled, IEnumerable<PayoutDefinition> payouts, AppleStorePromotionVisibility applePromotionVisibility = AppleStorePromotionVisibility.Default) : base(id, storeSpecificId, type, enabled, payouts)
        {
            this.applePromotionVisibility = applePromotionVisibility;
        }
    }
}