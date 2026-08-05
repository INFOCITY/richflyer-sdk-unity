//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using UnityEngine;

namespace RichFlyer
{
    public class RFMessageBridge : MonoBehaviour
    {
        private static RFMessageBridge _instance;

        public static RFMessageBridge Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Only one RFMessageBridge can be active. The duplicate component will be removed.");
                if (gameObject.name == _instance.gameObject.name)
                {
                    gameObject.name += " (Duplicate)";
                }
                Destroy(this);
                return;
            }

            _instance = this;

            GameObject persistentRoot = transform.root.gameObject;
            if (persistentRoot != gameObject)
            {
                Debug.LogWarning("RFMessageBridge is attached to a child GameObject. Its root GameObject will persist across scenes.");
            }
            DontDestroyOnLoad(persistentRoot);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        void onRichFlyerAction(string action)
        {
            RFPluginScript.BridgeAction(action, null);
        }

        void onRichFlyerExtendedProperty(string extendedProperty)
        {
            RFPluginScript.BridgeAction(null, extendedProperty);
        }

    }
}
