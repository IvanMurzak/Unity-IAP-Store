# Unity IAP Store
![npm](https://img.shields.io/npm/v/extensions.unity.iap.store)

Powerful Store manager for Unity project. You have codeless products management system, where you can create in-game products. Create all currencies with custom names and icons. Each product has price in single or multiple in-game currencies. Ability to create bundle of different products and sell them in single purchase. Any product can be easily swapped to IAP or back to in-game product. Fully supported IAP for iOS and Android out of the box, other platforms supported also, just need to extend fram base class and add needed to you code.

# How to use


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
