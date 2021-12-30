using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using BigInt = System.Numerics.BigInteger;

namespace Project.Store
{
	[Serializable]
	public class Price
	{
		bool        ValidateCurrency    (string x)  => StoreSO.Instance.ValidateCurrency(x);
		string[]    AllCurrenciesNames	()          => StoreSO.Instance.AllCurrenciesNames();

		[NonSerialized, OdinSerialize, HorizontalGroup(" ")]					protected BigInt amount;

		[ValidateInput("ValidateCurrency"), HideLabel, HorizontalGroup(" "), SerializeField, Required]
		[ValueDropdown("AllCurrenciesNames", IsUniqueList = true)]				protected string currency;

		public virtual BigInt Amount	=> amount;
		public virtual string Currency	=> currency;

		public Price() { }
		public Price(string currency, BigInt amount)
		{
			this.amount		= amount;
			this.currency	= currency;
		}

		public override string ToString() => $"{{amount={Amount}, currency={Currency}}}";
	}
}