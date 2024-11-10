using System;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using BigInt = System.Numerics.BigInteger;

namespace Project.Store
{
    [Serializable]
    public class StoreSubSellable : IStoreSellable
    {
        [BoxGroup(" ", ShowLabel = false), SerializeField, Required]            public          string  id;
        [BoxGroup(" ", ShowLabel = false), SerializeField]                      protected       string  title;
        [BoxGroup(" ", ShowLabel = false), SerializeField]                      protected       Sprite  sprite;
        [BoxGroup(" ", ShowLabel = false), NonSerialized, OdinSerialize]        protected       BigInt  quantity    = 1;
    
                                                                                public virtual  string  ID          => id;
                                                                                public virtual  string  Title       => title;
                                                                                public virtual  Sprite  Sprite      => sprite;
                                                                                public virtual  BigInt  Quantity    => quantity;
    }
}
