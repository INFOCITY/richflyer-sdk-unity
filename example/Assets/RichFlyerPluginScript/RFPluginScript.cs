//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System.Collections.Generic;

namespace RichFlyer
{
    public class RFPluginScript
    {
        public static void Initialize(string targetObjectName, RFNotificationReceiver receiver, RFCompleted onResult)
        {
#if UNITY_IOS
            RFIOSPluginScript.Initialize(receiver, onResult);
#elif UNITY_ANDROID
            RFAndroidPluginScript.Initialize(targetObjectName, receiver, onResult);
#endif
        }

        public static void RegistSegments(RFSegment[] segments, RFCompleted onResult)
        {
#if UNITY_IOS
            RFIOSPluginScript.RegistSegments(segments, onResult);
#elif UNITY_ANDROID
            RFAndroidPluginScript.RegistSegments(segments, onResult);
#endif

        }

        public static RFSegment[] GetSegments()
        {
#if UNITY_IOS
            return RFIOSPluginScript.GetSegments();
#elif UNITY_ANDROID            
            return RFAndroidPluginScript.GetSegments();
#else
            return null;
#endif
        }

        public static RFContent[] GetReceivedData()
        {
#if UNITY_IOS
            return RFIOSPluginScript.GetReceivedData();
#elif UNITY_ANDROID
            return RFAndroidPluginScript.GetReceivedData();
#else
            return null;
#endif
        }

        public static RFContent GetLatestReceivedData()
        {
#if UNITY_IOS
            return RFIOSPluginScript.GetLatestReceivedData();
#elif UNITY_ANDROID
            return RFAndroidPluginScript.GetLatestReceivedData();
#else
            return null;
#endif
        }

        public static void SetLaunchMode(int modes)
        {

#if UNITY_IOS
            RFIOSPluginScript.SetLaunchMode(modes);
#elif UNITY_ANDROID
            RFAndroidPluginScript.SetLaunchMode(modes);
#endif
        }

        public static void DisplayContent(string notificationId, RFContentDisplayCallback callback)
        {

#if UNITY_IOS
            RFIOSPluginScript.DisplayContent(notificationId, callback);
#elif UNITY_ANDROID
            RFAndroidPluginScript.DisplayContent(notificationId, callback);
#endif
        }

        public static void PostMessage(string[] events, Dictionary<string, string> variables, int? standbyTime, RFPostMessageCallback onResult)
        {
#if UNITY_IOS
            RFIOSPluginScript.PostMessage(events, variables, standbyTime, onResult);
#elif UNITY_ANDROID
            RFAndroidPluginScript.PostMessage(events, variables, standbyTime, onResult);
#endif
        }

        public static void CancelPosting(string eventPostId, RFPostMessageCallback onResult)
        {
#if UNITY_IOS
            RFIOSPluginScript.CancelPosting(eventPostId, onResult);
#elif UNITY_ANDROID
            RFAndroidPluginScript.CancelPosting(eventPostId, onResult);
#endif

        }


        // android only
        public static void BridgeAction(string actionJson, string extendedProperty)
        {
#if UNITY_ANDROID
            RFAndroidPluginScript.HandleAction(actionJson, extendedProperty);
#endif
        }

        // ios only
        public static void ResetBadgeNumber()
        {
#if UNITY_IOS
            RFIOSPluginScript.ResetBadgeNumber();
#endif
        }

        // ios only
        public static void SetBadgeNumber(int number)
        {
#if UNITY_IOS
            RFIOSPluginScript.SetBadgeNumber(number);
#endif
        }

    }

}
