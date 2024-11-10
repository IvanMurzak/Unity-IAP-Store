using UnityEngine.Purchasing;

namespace Project.Store
{
    public class TransactionContainer
    {
        public string productId;
        public string transactionId;
        public PurchaseFailureReason failureReason;
    }
}
