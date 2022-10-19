//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace RichFlyer
{
    public class RFProjectSettings : ScriptableObject
    {
        public string m_sdkkey;
        public int m_launchMode = -1;
        public string m_groupId;
        public bool m_displayNavigate;
        public int m_environment;
        public string m_themeColor;
  
        public static RFProjectSettings GetOrCreate()
        {
            var resourceDir = "Resources";
            var assetDir = "Assets";
            var assetName = "RFSettings.asset";

             
            var path = Path.Combine(assetDir, resourceDir , assetName);

            var absolutePath = Path.Combine(Application.dataPath, resourceDir);
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }


            var settings = AssetDatabase.LoadAssetAtPath<RFProjectSettings>(path);

            if (settings != null) return settings;

            settings = CreateInstance<RFProjectSettings>();
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            return settings;
        }

        // ScriptableObject を SerializedObject に変換して返します
        public static SerializedObject GetSerializedObject()
        {
            return new SerializedObject(GetOrCreate());
        }
    }

}
