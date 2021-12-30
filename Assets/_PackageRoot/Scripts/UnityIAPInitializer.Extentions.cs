using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using Newtonsoft.Json;

namespace Project.Store
{
    public partial class UnityIAPInitializer : IStoreListener
    {
        private bool CheckIfProductIsAvailableForSubscriptionManager(Product product)
        {
            if (product == null)
            {
                Debug.LogError("Product can't be null");
                return false;
            }
            if (product.definition.type != ProductType.Subscription) 
                return false;

            return CheckIfProductIsAvailableForSubscriptionManager(product.receipt);
        }
        private bool CheckIfProductIsAvailableForSubscriptionManager(string receipt)
        {
            if (string.IsNullOrEmpty(receipt)) 
                return false;

            var receipt_wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt);
            if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
            {
                Debug.Log("The product receipt does not contain enough information");
                return false;
            }
            var store = (string)receipt_wrapper["Store"];
            var payload = (string)receipt_wrapper["Payload"];

            if (payload != null)
            {
                switch (store)
                {
                    case GooglePlay.Name:
                        {
                            var payload_wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);
                            if (!payload_wrapper.ContainsKey("json"))
                            {
                                Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
                                return false;
                            }
                            var original_json_payload_wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)payload_wrapper["json"]);
                            if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload"))
                            {
                                Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
                                return false;
                            }
                            var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
                            var developerPayload_wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(developerPayloadJSON);
                            if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial"))
                            {
                                Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                                return false;
                            }
                            return true;
                        }
                    case AppleAppStore.Name:
                    case AmazonApps.Name:
                    case MacAppStore.Name:
                        {
                            return true;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
            return false;
        }
    }
}