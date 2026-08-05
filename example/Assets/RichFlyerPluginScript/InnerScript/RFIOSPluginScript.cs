//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

#if UNITY_IOS

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace RichFlyer
{
    public class RFIOSPluginScript
    {

        static RFNotificationReceiver _receiver;
        static readonly object CallbackLock = new object();
        static readonly Dictionary<long, RFCompleted> ResultCallbacks = new Dictionary<long, RFCompleted>();
        static readonly Dictionary<long, RFContentDisplayCallback> DisplayCallbacks = new Dictionary<long, RFContentDisplayCallback>();
        static readonly Dictionary<long, RFPostMessageCallback> PostMessageCallbacks = new Dictionary<long, RFPostMessageCallback>();
        static long _nextRequestId;

        public static void Initialize(RFNotificationReceiver receiver, RFCompleted onResult)
        {
            if (receiver == null)
            {
                onResult?.Invoke(false, 400, "Receiver not found.");
                return;
            }
            _receiver = receiver;
            registReceiver(NotificationReceiver);
            long requestId = AddCallback(ResultCallbacks, onResult);
            initializeRichFlyer(requestId, OnResultCallback);
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
            //segments to json
            var jsonDict = new RFSegmentsJson(segments ?? Array.Empty<RFSegment>());
            string segmentsJson = JsonUtility.ToJson(jsonDict);
            long requestId = AddCallback(ResultCallbacks, onResult);
            registSegments(segmentsJson, requestId, OnResultCallback);
        }

        public static RFSegment[] GetSegments()
        {
            string segmentsJson = GetNativeString(getSegments());
            RFSegmentsJson segmentObj = string.IsNullOrEmpty(segmentsJson)
                ? null
                : JsonUtility.FromJson<RFSegmentsJson>(segmentsJson);
            return segmentObj?.Segments ?? Array.Empty<RFSegment>();
        }

        public static RFContent[] GetReceivedData()
        {
            string contentsJson = GetNativeString(getReceivedData());
            RFContentArrayJson obj = string.IsNullOrEmpty(contentsJson)
                ? null
                : JsonUtility.FromJson<RFContentArrayJson>(contentsJson);
            return obj?.Contents ?? Array.Empty<RFContent>();
        }

        public static RFContent GetLatestReceivedData()
        {
            string contentJson = GetNativeString(getLatestReceivedData());
            return string.IsNullOrEmpty(contentJson) ? null : JsonUtility.FromJson<RFContent>(contentJson);
        }

        public static void SetLaunchMode(int modes)
        {
            setLaunchMode(modes);
        }

        public static void DisplayContent(string notificationId, RFContentDisplayCallback callback)
        {
            long requestId = AddCallback(DisplayCallbacks, callback);
            displayContent(notificationId, requestId, OnDismissContentDisplay);
        }

        public static void PostMessage(string[] events, Dictionary<string, string> variables, int? standbyTime, RFPostMessageCallback onResult)
        {
            RFEventArrayJson eventArrayJson = new RFEventArrayJson(events ?? Array.Empty<string>());
            string eventsJson = JsonUtility.ToJson(eventArrayJson);

            List<RFVariable> rfVariables = new List<RFVariable>();
            foreach (var variable in variables ?? new Dictionary<string, string>())
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
            long requestId = AddCallback(PostMessageCallbacks, onResult);
            postMessage(eventsJson, variableJson, rfStandbyTime, requestId, OnPostMessageCallback);
        }

        public static void CancelPosting(string eventPostId, RFPostMessageCallback onResult)
        {
            long requestId = AddCallback(PostMessageCallbacks, onResult);
            cancelPosting(eventPostId, requestId, OnPostMessageCallback);
        }


#region P/Invoke

        [DllImport("__Internal")]
        private static extern void registReceiver(RFNotificationReceiver receiver);

        [DllImport("__Internal")]
        private static extern void initializeRichFlyer(long requestId, RFCompletedNativeCallback onResult);

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

        private delegate void RFCompletedNativeCallback([MarshalAs(UnmanagedType.I1)] bool result, long code, string message, long requestId);

        [AOT.MonoPInvokeCallback(typeof(RFCompletedNativeCallback))]
        private static void OnResultCallback(bool result, long code, string message, long requestId)
        {
            TakeCallback(ResultCallbacks, requestId)?.Invoke(result, code, message);
        }

        [DllImport("__Internal")]
        private static extern void registSegments(string segments, long requestId, RFCompletedNativeCallback onResult);

        [DllImport("__Internal")]
        private static extern IntPtr getSegments();

        [DllImport("__Internal")]
        private static extern IntPtr getReceivedData();

        [DllImport("__Internal")]
        private static extern IntPtr getLatestReceivedData();

        [DllImport("__Internal")]
        private static extern void releaseString(IntPtr value);

        [DllImport("__Internal")]
        private static extern void setLaunchMode(int mode);

        private delegate void RFContentDisplayNativeCallback(string buttonTitle, string buttonValue, string buttonValueType, ulong buttonIndex, long requestId);

        [AOT.MonoPInvokeCallback(typeof(RFContentDisplayNativeCallback))]
        private static void OnDismissContentDisplay(string buttonTitle, string buttonValue, string buttonValueType, ulong buttonIndex, long requestId)
        {
            TakeCallback(DisplayCallbacks, requestId)?.Invoke(buttonTitle, buttonValue, buttonValueType, buttonIndex);
        }

        [DllImport("__Internal")]
        private static extern void displayContent(string notificationId, long requestId, RFContentDisplayNativeCallback callback);


        private delegate void RFPostMessageNativeCallback([MarshalAs(UnmanagedType.I1)] bool result, long code, string message, string eventPostIds, long requestId);
        [AOT.MonoPInvokeCallback(typeof(RFPostMessageNativeCallback))]
        private static void OnPostMessageCallback(bool result, long code, string message, string eventPostIds, long requestId)
        {
            RFPostMessageCallback callback = TakeCallback(PostMessageCallbacks, requestId);
            if (callback == null)
            {
                return;
            }

            if (eventPostIds != null && eventPostIds.Length > 0)
            {
                string[] eventPostIdArray = eventPostIds.Split(',');
                callback(result, code, message, eventPostIdArray);
            } else
            {
                callback(result, code, message, null);
            }
        }

        [DllImport("__Internal")]
        private static extern void postMessage(string events, string variables, int standbyTime, long requestId, RFPostMessageNativeCallback callback);

        [DllImport("__Internal")]
        private static extern void cancelPosting(string eventPostId, long requestId, RFPostMessageNativeCallback callback);

#endregion P/Invoke

        private static long AddCallback<T>(Dictionary<long, T> callbacks, T callback) where T : class
        {
            long requestId = Interlocked.Increment(ref _nextRequestId);
            if (callback != null)
            {
                lock (CallbackLock)
                {
                    callbacks[requestId] = callback;
                }
            }
            return requestId;
        }

        private static T TakeCallback<T>(Dictionary<long, T> callbacks, long requestId) where T : class
        {
            lock (CallbackLock)
            {
                T callback;
                if (!callbacks.TryGetValue(requestId, out callback))
                {
                    return null;
                }
                callbacks.Remove(requestId);
                return callback;
            }
        }

        private static string GetNativeString(IntPtr nativeString)
        {
            if (nativeString == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                int length = 0;
                while (Marshal.ReadByte(nativeString, length) != 0)
                {
                    length++;
                }

                var bytes = new byte[length];
                Marshal.Copy(nativeString, bytes, 0, length);
                return Encoding.UTF8.GetString(bytes);
            }
            finally
            {
                releaseString(nativeString);
            }
        }
    }
}

#endif
