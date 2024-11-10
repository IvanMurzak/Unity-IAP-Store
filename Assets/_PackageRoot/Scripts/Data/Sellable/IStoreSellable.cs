using UnityEngine;
using BigInt = System.Numerics.BigInteger;

namespace Project.Store
{
    public interface IStoreSellable
    {
        string ID       { get; }
        string Title    { get; }
        Sprite Sprite   { get; }
        BigInt Quantity { get; }
    }
}
