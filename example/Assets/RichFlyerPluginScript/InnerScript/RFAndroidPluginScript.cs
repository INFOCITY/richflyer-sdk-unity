//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

#if UNITY_ANDROID
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

namespace RichFlyer
{
    public class RFAndroidPluginScript
    {
        static RFNotificationReceiver _receiver;
        static RFContentDisplayCallback _displayCallback;

        /// <summary>
        /// Initialize fcm and richflyer.
        /// </summary>
        /// <param name="onResult">callback result.</param>
        public static void Initialize(string objectName, RFNotificationReceiver receiver, RFCompleted onResult)
        {
            SaveRFPreferences("RFContentCallbackTargetObject", objectName);

            if (receiver == null)
            {
                onResult(false, 400, "Receiver not found.");
                return;
            }
            _receiver = receiver;

            // Initialize fcm.
            InitializeFCM((bool result, long code, string message) =>
            {
                if (result)
                {
                    // Initialize RichFlyer
                    InitializeRichFlyer((bool result, long code, string message) =>
                    {
                        onResult.Invoke(result, code, message);
                    });
                } else
                {
                    Debug.LogError(message);
                }
            });
        }

        /// <summary>
        /// Register segment.
        /// </summary>
        /// <param name="segments">segment that is Dictionary object.</param>
        /// <param name="onResult">callback result</param>
        public static void RegistSegments(RFSegment[] segments, RFCompleted onResult)
        {
            var segmentsDict = new Dictionary<string, string>();
            foreach (RFSegment segment in segments)
            {
                segmentsDict.Add(segment.Name, segment.Value);
            }

            //convert dictionary
            AndroidJavaObject javaSegmnts = ConvertDictionaryToJavaMap(segmentsDict);
            GetRichFlyerJavaClass().CallStatic("registerSegments",
                new object[] { javaSegmnts, GetApplicationContext(), new RFResultListener(onResult) });
        }

        public static RFSegment[] GetSegments()
        {
            AndroidJavaObject mapSegment = GetRichFlyerJavaClass().CallStatic<AndroidJavaObject>("getSegments", new object[] { GetApplicationContext()});
            AndroidJavaObject keySet = mapSegment.Call<AndroidJavaObject>("keySet");
            string[] keys = keySet.Call<string[]>("toArray");

            List<RFSegment> segments = new List<RFSegment>();
            foreach (string key in keys)
            {
                string value = mapSegment.Call<string>("get", new object[] { key });
                RFSegment seg = new RFSegment((string)key, value);
                segments.Add(seg);                
            }

            return segments.ToArray();
        }

        public static RFContent[] GetReceivedData()
        {
            AndroidJavaObject contentList = GetRichFlyerJavaClass().CallStatic<AndroidJavaObject>("getHistory", new object[] { GetApplicationContext() });
            AndroidJavaObject[] contentArray = contentList.Call<AndroidJavaObject[]>("toArray");

            if (contentArray == null || contentArray.Length == 0)
            {
                return null;
            }

            List<RFContent> rfContentList = new List<RFContent>(contentArray.Length);

            foreach (AndroidJavaObject content in contentArray)
            {
                RFContent rfContent = convertRFContent(content);
                rfContentList.Add(rfContent);
            }
            
            return rfContentList.ToArray();
        }

        public static RFContent GetLatestReceivedData()
        {
            RFContent[] contents = GetReceivedData();
            if (contents == null || contents.Length == 0)
            {
                return null;
            }
            return contents[0];
        }

        public static void SetLaunchMode(int modes)
        {
            List<string> selectedModes = new List<string>();
            if ((modes & (int)RFLaunchModes.RFLaunchModeText) > 0)
            {
                selectedModes.Add("text");
            }

            if ((modes & (int)RFLaunchModes.RFLaunchModeImage) > 0)
            {
                selectedModes.Add("image");
            }

            if ((modes & (int)RFLaunchModes.RFLaunchModeGif) > 0)
            {
                selectedModes.Add("gif");
            }

            if ((modes & (int)RFLaunchModes.RFLaunchModeMovie) > 0)
            {
                selectedModes.Add("movie");
            }

            GetRichFlyerJavaClass().CallStatic("setLaunchMode", new object[] { GetApplicationContext(), selectedModes.ToArray() });
        }

        public static void DisplayContent(string notificationId, RFContentDisplayCallback callback)
        {
            _displayCallback = callback;
            GetRichFlyerJavaClass().CallStatic("showHistoryNotification", new object[] { GetApplicationContext(), notificationId });
        }

        public static void HandleAction(string actionJson, string extendedProperty)
        {
            RFAction action = JsonUtility.FromJson<RFAction>(actionJson);
            if (_displayCallback != null)
            {
                _displayCallback(action.Title, action.Value, action.Type, (ulong)action.Index);
                _displayCallback = null;
            }
            else
            {
                if (_receiver != null) {
                    _receiver(action.Title, action.Value, action.Type, (ulong)action.Index, extendedProperty);
                }
            }
        }

        private static void SaveRFPreferences(string key, string value)
        {
            AndroidJavaObject sharedPreference = GetApplicationContext().Call<AndroidJavaObject>("getSharedPreferences",
                new object[] { "jp.co.infocity.richflyer.preferences", 0});
            AndroidJavaObject editor = sharedPreference.Call<AndroidJavaObject>("edit");
            editor.Call<AndroidJavaObject>("putString", new object[] { key, value });
            editor.Call("apply");
        }

        private static RFContent convertRFContent(AndroidJavaObject javaRFContent)
        {
            RFContent content = new RFContent();
            content.Title = javaRFContent.Call<string>("getTitle");
            content.Body = javaRFContent.Call<string>("getMessage");
            content.ImagePath = javaRFContent.Call<string>("getImagePath");
            content.NotificationDateUnixTime = javaRFContent.Call<long>("getNotificationDate");
            content.ReceivedDateUnixTime = javaRFContent.Call<long>("getReceivedDate");
            content.NotificationId = javaRFContent.Call<string>("getNotificationId");
            content.Type = (RFContentType)javaRFContent.Call<int>("getContentType");


            AndroidJavaObject[] actions = javaRFContent.Call<AndroidJavaObject[]>("getActionButtonArray");
            if (actions.Length > 0)
            {
                List<RFAction> rfActions = new List<RFAction>(actions.Length);
                foreach (AndroidJavaObject actionObject in actions)
                {
                    string title = actionObject.Call<string>("getLabel");
                    string value = actionObject.Call<string>("getAction");
                    string type = actionObject.Call<string>("getActionType");
                    int index = actionObject.Call<int>("getIndex");
                    RFAction action = new RFAction(title, type, value, index);
                    rfActions.Add(action);
                }
                content.ActionButtons = rfActions.ToArray();
            }

            return content;
        }

        private class RFResultListener : AndroidJavaProxy
        {
            private RFCompleted _handler;
            public RFResultListener(RFCompleted onResult) : base("jp.co.infocity.richflyer.RichFlyerResultListener")
            {
                _handler = onResult;
            }

            public void onCompleted(AndroidJavaObject result)
            {
                if (_handler != null)
                {
                    bool boolReslt = result.Call<bool>("isResult");
                    int errorCode = result.Call<int>("getErrorCode");
                    string errorMessage = result.Call<string>("getMessage");

                    _handler.Invoke(boolReslt, errorCode, errorMessage);
                }
            }
        }

        private static void InitializeFCM(RFCompleted onResult)
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {

                Firebase.DependencyStatus dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    //Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                    //firebase = Firebase.FirebaseApp.DefaultInstance;

                    Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
                     task => {
                         //LogTaskCompletion(task, "RequestPermissionAsync");
                     }
                    );
                    //isFirebaseInitialized = true;
                    onResult.Invoke(true, 0, "Initialized FCM succeeded.");
                }
                else
                {
                    onResult.Invoke(false, 0, "Initialized FCM failed.");
                }
            });

        }

        private static void InitializeRichFlyer(RFCompleted onResult)
        {
            Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
            {
                string token = task.Result;

                GetRichFlyerJavaClass().CallStatic("checkNotificationPermission", new object[] { GetCurrentActivity() });

                RFAndroidSettings settings = RFAndroidSettings.LoadFromAppAsset();
                string sdkKey = settings.sdkKey;
                string themeColor = settings.themeColor;
                int launchMode = settings.launchMode;

                AndroidJavaObject targetClass = GetCurrentActivity().Call<AndroidJavaObject>("getClass");

                AndroidJavaObject richflyer = new AndroidJavaObject("jp.co.infocity.richflyer.RichFlyer",
                    new object[] { GetApplicationContext(), token, sdkKey, themeColor, targetClass });

                if (richflyer != null)
                {
                    richflyer.Call("startSetting", new RFResultListener(onResult));

                    SetLaunchMode(launchMode);
                }
            });
        }

        private static AndroidJavaObject GetCurrentActivity()
        {
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return unity.GetStatic<AndroidJavaObject>("currentActivity");
        }

        private static AndroidJavaObject GetApplicationContext()
        {
            return GetCurrentActivity().Call<AndroidJavaObject>("getApplicationContext");
        }

        private static AndroidJavaClass GetRichFlyerJavaClass()
        {
            return new AndroidJavaClass("jp.co.infocity.richflyer.RichFlyer");
        }

        private static AndroidJavaObject ConvertDictionaryToJavaMap(Dictionary<string, string> dictionary)
        {

            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            System.IntPtr putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            foreach (var entry in dictionary)
            {
                AndroidJNI.CallObjectMethod(
                    map.GetRawObject(),
                    putMethod,
                    AndroidJNIHelper.CreateJNIArgArray(new object[] { entry.Key, entry.Value })
                );
            }
            return map;
        }

    }


}
#endif