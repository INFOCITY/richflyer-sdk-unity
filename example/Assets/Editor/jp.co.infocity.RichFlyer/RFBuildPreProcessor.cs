//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using RichFlyer;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class RFBuildPreProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 1;

#if UNITY_ANDROID
    private const string PLUGINS_DIR = "Assets/Plugins/Android/RichFlyer";
    private const string ICON_DIR = "Icon";
    private const string ICON_AAR = "notification_icon.aar";
#endif

    public void OnPreprocessBuild(BuildReport report)
    {
#if UNITY_ANDROID

        createSettingJson();

         // make resource aar
        createResourceAAR();

#endif
    }

    private void createSettingJson()
    {
#if UNITY_ANDROID
        RFProjectSettings settings = RFProjectSettings.GetOrCreate();
        var androidSettings = new RFAndroidSettings(settings.m_sdkkey, settings.m_themeColor, settings.m_launchMode);
        androidSettings.SaveStreamingAsset();
#endif
    }

    private void createResourceAAR()
    {
#if UNITY_ANDROID

        string iconDir = Path.Combine(PLUGINS_DIR, ICON_DIR);
        string aarPath = Path.Combine(PLUGINS_DIR, ICON_AAR);

        string aarAbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), aarPath);
        if (File.Exists(aarAbsolutePath))
        {
            File.Delete(aarAbsolutePath);
        }

        ZipFile.CreateFromDirectory(iconDir, aarPath);
        using (FileStream zipToOpen = new FileStream(aarPath, FileMode.Open))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
            {
                List<ZipArchiveEntry> deleteEntry = new List<ZipArchiveEntry>();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".meta") || entry.FullName.EndsWith(".DS_Store"))
                    {
                        deleteEntry.Add(entry);
                    }
                }

                foreach (ZipArchiveEntry entry in deleteEntry)
                {
                    entry.Delete();

                }
            }
        }
#endif
    }
}
