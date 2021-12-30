using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Store
{
	public class Currency
	{
		static void InvalidateData() => StoreSO.Instance.InvalidateData();

		[Required, OnValueChanged("InvalidateData")] public string name;
		[Required, OnValueChanged("InvalidateData")] public Sprite icon;
	}
}