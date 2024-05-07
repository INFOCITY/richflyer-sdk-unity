//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

#if UNITY_IOS

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RichFlyer
{
    public class RFIOSPluginScript
    {

        static RFCompleted _callback;
        static RFNotificationReceiver _receiver;
        static RFContentDisplayCallback _displayCallback;
        static RFPostMessageCallback _postMessageCallback;

        public static void Initialize(RFNotificationReceiver receiver, RFCompleted onResult)
        {
            if (receiver == null)
            {
                onResult(false, 400, "Receiver not found.");
                return;
            }
            _receiver = receiver;
            registReceiver(NotificationReceiver);
            onResult(true, 0, "");
        }

        public static void ResetBadgeNumber()
        {
            resetBadgeNumber();
        }

        public static void SetBadgeNumber(int number)
        {
            setBadgeNumber(number);
        }

        public static void RegistSegments(RFSegment[] segments, RFCompleted onResult)
        {
            _callback = onResult;

            //segments to json
            var jsonDict = new RFSegmentsJson(segments);
            string segmentsJson = JsonUtility.ToJson(jsonDict);
            registSegments(segmentsJson, OnResultCallback);
        }

        public static RFSegment[] GetSegments()
        {
            string segmentsJson = getSegments();
            RFSegmentsJson segmentObj = JsonUtility.FromJson<RFSegmentsJson>(segmentsJson);
            return segmentObj.Segments;
        }

        public static RFContent[] GetReceivedData()
        {
            string contentsJson = getReceivedData();
            RFContentArrayJson obj = JsonUtility.FromJson<RFContentArrayJson>(contentsJson);
            RFContent[] receivedContent = obj.Contents;
            return receivedContent;
        }

        public static RFContent GetLatestReceivedData()
        {
            string contentJson = getLatestReceivedData();
            RFContent content = JsonUtility.FromJson<RFContent>(contentJson);

            return content;
        }

        public static void SetLaunchMode(int modes)
        {
            setLaunchMode(modes);
        }

        public static void DisplayContent(string notificationId, RFContentDisplayCallback callback)
        {
            _displayCallback = callback;
            displayContent(notificationId, OnDismissContentDisplay);
        }

        public static void PostMessage(string[] events, Dictionary<string, string> variables, int? standbyTime, RFPostMessageCallback onResult)
        {
            _postMessageCallback = onResult;

            RFEventArrayJson eventArrayJson = new RFEventArrayJson(events);
            string eventsJson = JsonUtility.ToJson(eventArrayJson);

            List<RFVariable> rfVariables = new List<RFVariable>();
            foreach (var variable in variables)
            {
                RFVariable rfVariable = new RFVariable(variable.Key, variable.Value);
                rfVariables.Add(rfVariable);
            }
            RFVariablesJson rfVariablesJson = new RFVariablesJson(rfVariables.ToArray());
            string variableJson = JsonUtility.ToJson(rfVariablesJson);

            int rfStandbyTime = -1;
            if (standbyTime != null) {
                rfStandbyTime = standbyTime.Value;
            }
            postMessage(eventsJson, variableJson, rfStandbyTime, OnPostMessageCallback);
        }

        public static void CancelPosting(string eventPostId, RFPostMessageCallback onResult)
        {
            _postMessageCallback = onResult;
            cancelPosting(eventPostId, OnPostMessageCallback);
        }


#region P/Invoke

        [DllImport("__Internal")]
        private static extern void registReceiver(RFNotificationReceiver receiver);

        [AOT.MonoPInvokeCallback(typeof(RFNotificationReceiver))]
        private static void NotificationReceiver(string buttonTitle, string buttonValue, string buttonValueType, ulong buttonIndex, string extendedProperty)
        {
            if (_receiver != null) {
                _receiver(buttonTitle, buttonValue, buttonValueType, buttonIndex, extendedProperty);
            }
        }

        [DllImport("__Internal")]
        private static extern void resetBadgeNumber();

        [DllImport("__Internal")]
        private static extern void setBadgeNumber(int number);

        [AOT.MonoPInvokeCallback(typeof(RFCompleted))]
        private static void OnResultCallback(bool result, long code, string message)
        {
            _callback(result, code, message);
        }

        [DllImport("__Internal")]
        private static extern void registSegments(string segments, RFCompleted onResult);

        [DllImport("__Internal")]
        private static extern string getSegments();

        [DllImport("__Internal")]
        private static extern string getReceivedData();

        [DllImport("__Internal")]
        private static extern string getLatestReceivedData();

        [DllImport("__Internal")]
        private static extern void setLaunchMode(int mode);

        [AOT.MonoPInvokeCallback(typeof(RFContentDisplayCallback))]
        private static void OnDismissContentDisplay(string buttonTitle, string buttonValue, string buttonValueType, ulong buttonIndex)
        {
            _displayCallback(buttonTitle, buttonValue, buttonValueType, buttonIndex);
        }

        [DllImport("__Internal")]
        private static extern void displayContent(string notificationId, RFContentDisplayCallback callback);


        public delegate void RFPostMessageNativeCallback(bool result, long code, string message, string eventPostIds);
        [AOT.MonoPInvokeCallback(typeof(RFPostMessageNativeCallback))]
        private static void OnPostMessageCallback(bool result, long code, string message, string eventPostIds)
        {
            if (eventPostIds != null && eventPostIds.Length > 0)
            {
                string[] eventPostIdArray = eventPostIds.Split(',');
                _postMessageCallback(result, code, message, eventPostIdArray);
            } else
            {
                _postMessageCallback(result, code, message, null);
            }
        }

        [DllImport("__Internal")]
        private static extern void postMessage(string events, string variables, int standbyTime, RFPostMessageNativeCallback callback);

        [DllImport("__Internal")]
        private static extern void cancelPosting(string eventPostId, RFPostMessageNativeCallback callback);

#endregion P/Invoke
    }
}

#endif