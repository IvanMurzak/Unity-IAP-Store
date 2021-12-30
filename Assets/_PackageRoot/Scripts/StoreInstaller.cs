using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Store
{
    public class StoreInstaller : SerializedMonoBehaviour
    {
        [SerializeField, Required] StoreSO storeScriptableObject;
    }
}