using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Store
{
    public class SimplePurchaser : SerializedMonoBehaviour
    {
        protected bool      ValidateIDStr   (string x)  => StoreSO.Instance?.ValidateID(x) ?? false;
        protected string[]  AllSellableIDs  ()          => StoreSO.Instance?.AllSellableIDs() ?? new string[] { };

        [ValidateInput("ValidateIDStr")]
        [ValueDropdown("AllSellableIDs", IsUniqueList = true)]
        [SerializeField, Required] string productID;

        public void Purchase()
        {
            if (StoreSO.Instance == null)
            {
                Debug.LogError("StoreSO.Instance is not yet initialized");
            }
            else
            {
                StoreSO.Instance.Purchase(productID);
            }
        }
    }
}
