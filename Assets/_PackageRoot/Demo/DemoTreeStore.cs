using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using UniRx;
using Project.Store;
using BigInt = System.Numerics.BigInteger;

[CreateAssetMenu(fileName = "DemoTreeStore", menuName = "Store/DemoTreeStore")]
public class DemoTreeStore : StoreSO
{
    [OdinSerialize, NonSerialized]
    public ReactiveProperty<BigInt> balance = new ReactiveProperty<BigInt>();

    public override IObservable<Price> OnBalanceChanged(string currency)
    {
        return balance.Select(x => new Price(currency, x));
    }

    public override BigInt GetBalance(string currency)
    {
        return balance.Value;
    }

    protected override void SpendBalance(string currency, BigInt amount)
    {
        balance.Value -= amount;
    }
    protected override void ApplyPurchase(List<IStoreSellable> sellables)
    {
        // Apply purchase here
    }
}