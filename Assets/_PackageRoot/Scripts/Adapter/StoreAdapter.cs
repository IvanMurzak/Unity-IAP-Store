using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Store
{ 
	public abstract class StoreAdapter : SerializedMonoBehaviour
	{
		[FoldoutGroup("Settings")]							public		bool						refreshOnStart = true;
		[FoldoutGroup("Settings"), Required, AssetsOnly]	public		StoreSellableDrawer			drawerPrefab;
		[SerializeField, HideInInspector]					protected	List<StoreSellableDrawer>	instances = new List<StoreSellableDrawer>();

		protected virtual void Start()
		{
			if (refreshOnStart) Refresh();
		}

		[Button(ButtonSizes.Medium), HorizontalGroup(" ")]
		public virtual void Refresh()
		{
			if (instances.Any(x => x == null))
				instances = GetComponentsInChildren<StoreSellableDrawer>().ToList();

			var sellables = GetSellables();
			for(int i = 0; i < sellables.Count; i++)
			{
				if (instances.Count <= i) instances.Add(InstantiateDrawer(drawerPrefab));
				instances[i].ID = sellables[i].ID;
				instances[i].UpdateDrawer();
			}

			var removeRange = instances.Count - sellables.Count;
			for (int i = sellables.Count; i < instances.Count; i++)
				DestroyDrawerInstance(instances[i]);

			if (removeRange > 0) instances.RemoveRange(sellables.Count, removeRange);
		}

		[Button(ButtonSizes.Medium), HorizontalGroup(" "), GUIColor(1f, .5f, .5f)]
		public virtual void ClearInstances()
		{
			for (int i = 0; i < instances.Count; i++)
				DestroyDrawerInstance(instances[i]);

			instances.Clear();
		}

		protected virtual StoreSellableDrawer InstantiateDrawer(StoreSellableDrawer drawerPrefab)
		{
	#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				var gameObjectInstance = UnityEditor.PrefabUtility.InstantiatePrefab(drawerPrefab.gameObject, transform) as GameObject;
				return gameObjectInstance.GetComponent<StoreSellableDrawer>();
			}
	#endif
			return Instantiate(drawerPrefab, transform);
		}
		protected virtual void DestroyDrawerInstance(StoreSellableDrawer instance)
		{
			if (Application.isPlaying)	Destroy(instance.gameObject);
			else						DestroyImmediate(instance.gameObject);
		}

		protected abstract List<StoreSellable> GetSellables();
	}
}