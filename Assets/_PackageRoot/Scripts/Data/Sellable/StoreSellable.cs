using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using BigInt = System.Numerics.BigInteger;

namespace Project.Store
{
	[Serializable]
	public class StoreSellable : IStoreSellable
	{
		static void InvalidateData() => StoreSO.Instance.InvalidateData();

		//[OdinSerialize, NonSerialized]
		//public BigInt bigIntTest = 100;

		[OnValueChanged("InvalidateData"), SerializeField, Required]													protected		string							id;
		[Space]
		[SerializeField]																								protected		string							title;
		[SerializeField]																								protected		Sprite							sprite;
		[NonSerialized, OdinSerialize]																					protected		BigInt							quantity							= 1;
		[Space]
		[OnValueChanged("InvalidateData")]																				public			bool							isIAP;

		[ShowIfGroup("IAP", MemberName = "isIAP")]
		[BoxGroup("IAP/_", centerLabel: true, GroupName = "IAP")]
		[OnValueChanged("InvalidateData"), Required("Can't be empty"), LabelText("Store specific ID"), SerializeField]	protected		string							IAP_StoreSpecificID;

		[BoxGroup("IAP/_"), LabelText("Product Type"),		OdinSerialize]												public			ProductType						IAP_ProductType						{ get; protected set; }
		[BoxGroup("IAP/_"), LabelText("Payout Definition"), OdinSerialize, HideReferenceObjectPicker, Required]			public			PayoutDefinition				IAP_PayoutDefinition				{ get; protected set; }
		[BoxGroup("IAP/_"), LabelText("AppleStore Promotion Visibility"), OdinSerialize, LabelWidth(250)]				public			AppleStorePromotionVisibility	applePromotionVisibility			{ get; protected set; }
		
		[BoxGroup("IAP/_"), LabelText("Override IDs"), SerializeField]													protected		bool							IAP_OverrideIDs;

		[ShowIfGroup("IAP/_/Override", MemberName = "IAP_OverrideIDs")]
		[BoxGroup("IAP/_/Override/IDs", centerLabel: true, GroupName = "Override IAP IDs")]
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					GooglePlay							= new OverridedIAP();
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					IOSAppStore							= new OverridedIAP();
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					MacAppStore							= new OverridedIAP();
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					UniversalWindowsPlatform			= new OverridedIAP();
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					AmazonAppstore						= new OverridedIAP();
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					SamsungGalaxyApps					= new OverridedIAP();
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					TizenStore							= new OverridedIAP();
		[BoxGroup("IAP/_/Override/IDs"), SerializeField, HideReferenceObjectPicker, InlineProperty, LabelWidth(170)]	protected		OverridedIAP					CloudMoolahStore					= new OverridedIAP();

		[Space]
		[HideIf("isIAP"), HideReferenceObjectPicker]																	public			List<Price>						required							= new List<Price>() { new Price() };
		[Space]
		[Required, HideReferenceObjectPicker, SerializeField]															protected		List<StoreSubSellableContainer>	subSellables						= new List<StoreSubSellableContainer>();
	
																														public virtual	string							ID									=> id;
																														public virtual	string							Title								=> title;
																														public virtual	string							IAP_StoreSpecitifID					=> isIAP ? IAP_StoreSpecificID : null;
																														public virtual	Sprite							Sprite								=> sprite;
																														public virtual	BigInt							Quantity							=> quantity;
																														public virtual	IEnumerable<IStoreSellable>		SubSellables						=> subSellables.Select(x =>
																														{
																															if (x.type == StoreSubSellableContainer.SubType.Link)		return StoreSO.Instance.GetSellable(x.id) as IStoreSellable;
																															if (x.type == StoreSubSellableContainer.SubType.Instance)	return x.instance;
																															return null;
																														}).Where(x => x != null);

		public StoreSellable() { }
		public StoreSellable(string id, string title, Sprite sprite, BigInt quantity, bool isIAP = false, string IAP_StoreSpecificID = "", ProductType IAP_ProductType = ProductType.Consumable, PayoutDefinition IAP_PayoutDefinition = null)
		{
			this.id						= id;
			this.title					= title;
			this.sprite					= sprite;
			this.quantity				= quantity;
			this.isIAP					= isIAP;
			this.IAP_StoreSpecificID	= IAP_StoreSpecificID;
			this.IAP_ProductType		= IAP_ProductType;
			this.IAP_PayoutDefinition	= IAP_PayoutDefinition;
		}


		[Serializable]
		public class OverridedIAP
		{
			[HorizontalGroup(" "), HideLabel]							public bool		isOverriden		= false;
			[Required("Can't be empty")]
			[HorizontalGroup(" "), ShowIf("isOverriden"), HideLabel]	public string	IAP_ID			= "";
		}
		[Serializable]
		public class StoreSubSellableContainer
		{
			protected bool		ValidateID(string x)	=> StoreSO.Instance.ValidateID(x);
			protected string[]	AllSellableIDs()		=> StoreSO.Instance.AllSellableIDs();

			public SubType			type;

			[ValueDropdown("AllSellableIDs", IsUniqueList = true)]
			[Required, ShowIf("@type == SubType.Link")]
			[ValidateInput("ValidateIDStr")]
			public string			id;

			[Required, ShowIf("@type == SubType.Instance")]
			public StoreSubSellable instance;

			public enum SubType
			{
				Link, Instance
			}
		}
	}
}