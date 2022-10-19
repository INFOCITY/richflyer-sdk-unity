//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

#if UNITY_ANDROID
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
            var path = Path.Combine(Application.streamingAssetsPath, RICHFLYER_ASSET_DIR, RICHFLYER_SETTING);

            UnityWebRequest request = UnityWebRequest.Get(path);
            request.SendWebRequest();
            while (!request.isDone) { }

            return JsonUtility.FromJson<RFAndroidSettings>(request.downloadHandler.text);
        }

    }
}
#endif