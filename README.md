# Unity IAP Store
![npm](https://img.shields.io/npm/v/extensions.unity.iap.store) ![License](https://img.shields.io/github/license/IvanMurzak/Unity-IAP-Store) [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

![image](https://user-images.githubusercontent.com/9135028/182879404-d7cbc547-5f3d-4b08-9185-fcaf10b0080c.png)


Powerful Store manager for Unity project. You have codeless products management system, where you can create in-game products. Create all currencies with custom names and icons. Each product has price in single or multiple in-game currencies. Ability to create bundle of different products and sell them in single purchase. Any product can be easily swapped to IAP or back to in-game product. Fully supported IAP for iOS and Android out of the box, other platforms supported also, just need to extend fram base class and add needed to you code.

## Features

- ✔️ add custom currencies
- ✔️ item price in multiple currencies 
- ✔️ item categorization
- ✔️ in-app purchases supported for iOS and Android
- ✔️ currencies management
- ✔️ pack of multiple products by single purchase
- ✔️ pack of multiple currencies by single purchase
- ✔️ drawer for showing items in Unity UI with ability to override for any other UI system
- ✔️ drawer adapter for showing all items from specific category in Unity UI with ability to override for any other UI system
- ✔️ drawer adapter for showing all items from specific category in Unity UI with ability to override for any other UI system


# How to install - Option 1 (RECOMMENDED)

- Install [ODIN Inspector](https://odininspector.com/)
- Install [OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open command line in Unity project folder
- `openupm --registry add extensions.unity.iap.store`

# How to install - Option 2

- Install [ODIN Inspector](https://odininspector.com/)
- Add this code to <code>/Packages/manifest.json</code>
```json
{
  "dependencies": {
    "extensions.unity.iap.store": "4.5.2",
  },
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.cysharp.unitask",
        "com.neuecc.unirx"
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

[CreateAssetMenu(fileName = "MyStore", menuName = "Store/MyStore")]
public class MyStore : StoreSO
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
```
### 2. Create instance of the new ScriptableObject `MyStore`
Create instance and do setup. You can add as many currencies as needed. Also you can use it without currencies at all, if you just need to handle in-app purchases.
![Unity_4gPx4Wi804](https://user-images.githubusercontent.com/9135028/182863155-054f4b69-085f-4cae-8e55-3e24b21e1127.gif)

### 3. Add StoreInstaller to any gameObject
![Unity_uV6ioqFm1l](https://user-images.githubusercontent.com/9135028/182876230-67e7bd27-418d-46ff-8e9c-710a8b2ebe2a.gif)


# How to show sellable item(s) in UI

This system is quite independent, but you need to show sellable items for a user, to make ability for a user to buy them. There are multiple ways to do that.

- create prefab for drawing UI element which represent single item for selling. If you need you can use multiple prefabs for different reasons in different places
- add `StoreSellableDrawer_UGUI` component to the prefab
- bind all required elements to the component

![image](https://user-images.githubusercontent.com/9135028/182894050-03907564-178f-4ea9-890c-056dcb8ba9a2.png)

It can be used for showing single sellable item.

## Adapters
Adapter generates multiples items, very easy to setup all of them from single place with small amount of setup steps.

- `StoreCategoryAdapter` - shows list of sellable items from specific category
- `StoreCustomAdapter` - shows custom list of sellable items
- Or create your own StoreAdapter, for that need to extand from `StoreAdapter`. Also you may create custom StoreSellableDrawer for any UI system in case if you don't use Unity UI.

![image](https://user-images.githubusercontent.com/9135028/182894888-15171454-b3e7-438b-9a35-dbd11f51c2cc.png)

# How to show player's current currencies balance in UI
`CurrencyBalanceDrawer_UGUI_TMP` show currency balance and refreshes it when it changed.

![image](https://user-images.githubusercontent.com/9135028/182909230-64a3e610-73b8-463d-8acf-d61c10d8f323.png)


# How to show separate price of specific sellable item

That is easy. Just create any gameObject with Text component and add `PriceDrawer` on it.

![image](https://user-images.githubusercontent.com/9135028/182895876-2cc5b343-b3c7-4a71-8552-1e5c259939ab.png)


# Other
- to execute `Purchase` action without sellable item drawer and/or adapter use `SimplePurchaser`
- to restore non-consumable in-app purchases use `SimpleInAppPurchaseRestorer`

![image](https://user-images.githubusercontent.com/9135028/182900204-5c9dddf7-87f6-4c34-b76d-6e70b1b8ae64.png)


