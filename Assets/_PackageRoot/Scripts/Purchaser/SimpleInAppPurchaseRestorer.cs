using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Store
{
    public class SimpleInAppPurchaseRestorer : SerializedMonoBehaviour
    {
        [Button(ButtonSizes.Medium)]
        public void Restore()
        {
            if (StoreSO.Instance == null)
            {
                Debug.LogError("StoreSO.Instance is not yet initialized");
            }
            else
            {
                StoreSO.Instance.RestorePurchases();
            }
        }
    }
}
