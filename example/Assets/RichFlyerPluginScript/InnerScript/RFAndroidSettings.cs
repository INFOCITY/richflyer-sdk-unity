//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

#if UNITY_ANDROID
using System.IO;
using System.Text;
using UnityEngine;

namespace RichFlyer
{
    public class RFAndroidSettings
    {
        public const string RICHFLYER_ASSET_DIR = "RichFlyer";
        public const string RICHFLYER_SETTING = "RFSetting.json";
        private const string STREAMING_ASSET_DIR = "Assets/StreamingAssets";

        public string sdkKey;
        public string themeColor;
        public int launchMode;

        public RFAndroidSettings(string sdkKey, string themeColor, int launchMode)
        {
            this.sdkKey = sdkKey;
            this.themeColor = themeColor;
            this.launchMode = launchMode;
        }

        public void SaveStreamingAsset()
        {
            string streamingAssetPath = Path.Combine(Directory.GetCurrentDirectory(), STREAMING_ASSET_DIR);
            if (!Directory.Exists(streamingAssetPath))
            {
                Directory.CreateDirectory(streamingAssetPath);
            }

            string richflyerAssetPath = Path.Combine(streamingAssetPath, RICHFLYER_ASSET_DIR);
            if (!Directory.Exists(richflyerAssetPath))
            {
                Directory.CreateDirectory(richflyerAssetPath);
            }

            string settingPath = Path.Combine(richflyerAssetPath, RICHFLYER_SETTING);
            string settingJson = JsonUtility.ToJson(this);
            File.WriteAllText(settingPath, settingJson);
        }

        public static RFAndroidSettings LoadFromAppAsset()
        {
            string assetPath = Path.Combine(RICHFLYER_ASSET_DIR, RICHFLYER_SETTING).Replace('\\', '/');

            using (AndroidJavaObject activity = GetCurrentActivity())
            using (AndroidJavaObject assets = activity.Call<AndroidJavaObject>("getAssets"))
            using (AndroidJavaObject stream = assets.Call<AndroidJavaObject>("open", assetPath))
            using (AndroidJavaObject streamReader = new AndroidJavaObject("java.io.InputStreamReader", stream, Encoding.UTF8.WebName))
            using (AndroidJavaObject reader = new AndroidJavaObject("java.io.BufferedReader", streamReader))
            {
                try
                {
                    var json = new StringBuilder();
                    string line;
                    while ((line = reader.Call<string>("readLine")) != null)
                    {
                        json.Append(line);
                    }

                    RFAndroidSettings settings = JsonUtility.FromJson<RFAndroidSettings>(json.ToString());
                    if (settings == null)
                    {
                        throw new InvalidDataException($"Invalid RichFlyer settings: {assetPath}");
                    }

                    return settings;
                }
                finally
                {
                    reader.Call("close");
                }
            }
        }

        private static AndroidJavaObject GetCurrentActivity()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

    }
}
#endif
