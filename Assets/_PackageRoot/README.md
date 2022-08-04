# Unity IAP Store
![npm](https://img.shields.io/npm/v/extensions.unity.iap.store)

Powerful Store manager for Unity project. You have codeless products management system, where you can create in-game products. Create all currencies with custom names and icons. Each product has price in single or multiple in-game currencies. Ability to create bundle of different products and sell them in single purchase. Any product can be easily swapped to IAP or back to in-game product. Fully supported IAP for iOS and Android out of the box, other platforms supported also, just need to extend fram base class and add needed to you code.

## Features

- ✔️ add custom currencies
- ✔️ item price in multiple currencies 
- ✔️ in-app purchases supported for iOS and Android
- ✔️ currencies management
- ✔️ pack of multiple products by single purchase
- ✔️ pack of multiple currencies by single purchase

# How to install

- Install [ODIN Inspector](https://odininspector.com/)
- Add this code to <code>/Packages/manifest.json</code>
```json
{
  "dependencies": {
    "extensions.unity.iap.store": "2.3.5",
  },
  "scopedRegistries": [
    {
      "name": "Unity Extensions",
      "url": "https://registry.npmjs.org",
      "scopes": [
        "extensions.unity"
      ]
    },
    {
      "name": "NPM",
      "url": "https://registry.npmjs.org",
      "scopes": [
        "com.cysharp",
        "com.neuecc"
      ]
    },
    {
      "name": "Packages from jillejr",
      "url": "https://npm.cloudsmith.io/jillejr/newtonsoft-json-for-unity/",
      "scopes": [
        "jillejr"
      ]
    }
  ]
}
```

# How to setup
### 1. Extends from `StoreSO`
Override at least abstract method. You should take of saving your data in persistent memory. There are basic methods required to implement in the example below.
```C#
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

    protected override BigInt GetBalance(string currency)
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
```
### 2. Create instance of the new ScriptableObject `DemoStoreSO`
Create instance and do setup. You can add as many currencies as needed. Also you can use it without currencies at all, if you just need to handle in-app purchases.
![Unity_4gPx4Wi804](https://user-images.githubusercontent.com/9135028/182863155-054f4b69-085f-4cae-8e55-3e24b21e1127.gif)

# How to use
