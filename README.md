# Unity IAP Store
![npm](https://img.shields.io/npm/v/extensions.unity.iap.store)


# How to use


# How to install
- Install [ODIN Inspector](https://odininspector.com/)
- Add this code to <code>/Packages/manifest.json</code>
```json
{
  "dependencies": {
    "extensions.unity.iap.store": "2.3.0",
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
