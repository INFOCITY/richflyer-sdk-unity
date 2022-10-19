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
