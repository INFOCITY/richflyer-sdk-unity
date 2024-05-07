//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using System.IO;
using System.IO.Compression;
using UnityEngine.Assertions;
using RichFlyer;

public class RFBuildPostProcessor
{
#if UNITY_IOS || UNITY_IPHONE
    private const string PLUGINS_DIR = "Plugins/iOS/RichFlyer";
    private const string FRAMEWORK_NAME = "RichFlyer.framework";
    private const string XC_FRAMEWORK_NAME = "RichFlyer.xcframework";

    private const string EXTENSION_DIR = "RFExtension";
    private const string NSE_NAME = "NotificationService";
    private const string NSE_HEADER_FILE = "NotificationService.h";
    private const string NSE_SOURCE_FILE = "NotificationService.m";

    private const string NCE_NAME = "NotificationContent";
    private const string NCE_HEADER_FILE = "NotificationViewController.h";
    private const string NCE_SOURCE_FILE = "NotificationViewController.m";
    private const string NCE_BASEPROJ_DIR = "Base.lproj";
    private const string NCE_STORYBORAD_FILE = "MainInterface.storyboard";

    private static readonly string FRAMEWORK_TARGET_PATH = Path.Combine("Frameworks", PLUGINS_DIR);
    private static readonly string XC_FRAMEWORK_SRC_DIR = Path.Combine("Assets", PLUGINS_DIR);
    private static readonly string LIBRARIES_EXTENSION_PATH = Path.Combine("Libraries", PLUGINS_DIR, EXTENSION_DIR);
    private static readonly string APPEXTENSION_SRC_DIR = Path.Combine(XC_FRAMEWORK_SRC_DIR, EXTENSION_DIR);
#endif

     
    /*
     * Player Settings - Mobile Notificationsで全てのチェックを外す
     * 
     */

    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        try
        {
#if UNITY_IOS || UNITY_IPHONE
            Debug.Log("Xcodeプロジェクト読み込み...");
            // Xcodeプロジェクトを読み込む
            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);

            string targetGuid = project.GetUnityMainTargetGuid();
            
            RFProjectSettings settings= RFProjectSettings.GetOrCreate();
            string rfAppGroup;
            if (settings.m_groupId == null || settings.m_groupId.Length == 0)
            {
                rfAppGroup = "group." + PlayerSettings.applicationIdentifier;
            } else
            {
                rfAppGroup = settings.m_groupId;
            }
            


            //RichFlyer.xcframeworkを追加
            string rfFrameworkGuid = addRichFlyerFramework(project, targetGuid, pathToBuiltProject);


            //Appターゲットの設定
            configureMainTarget(project, targetGuid, pathToBuiltProject, rfAppGroup);

            //UnityFrameworkターゲットの設定
            configureUnityFramework(project, targetGuid, rfFrameworkGuid);

            //Notification Service Extensionを追加
            addNotificationServiceFramework(project, targetGuid, pathToBuiltProject, rfAppGroup, rfFrameworkGuid);

            //Notification Content Extensionを追加
            addNotificationContentFramework(project, targetGuid, pathToBuiltProject, rfAppGroup, rfFrameworkGuid);

            //不要なファイルを削除
            removeUnusedFile(project, pathToBuiltProject);


            //Xcodeプロジェクトを設定保存
            project.WriteToFile(pbxProjectPath);
#endif

        }
        catch (System.Exception e)
        {
            Debug.LogError($"{e}");
        }

    }

#if UNITY_IOS || UNITY_IPHONE
    private static string addRichFlyerFramework(PBXProject project, string mainTargetGuid, string pathToBuiltProject)
    {
        //自動追加されたframeworkを削除
        string unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();

        string searchPath = Path.Combine(pathToBuiltProject, FRAMEWORK_TARGET_PATH, XC_FRAMEWORK_NAME);
        string[] deleteFrameworks = Directory.GetDirectories(searchPath, FRAMEWORK_NAME, SearchOption.AllDirectories);
        foreach (string framework in deleteFrameworks)
        {
            string projectPath = framework.Replace(pathToBuiltProject, "").TrimStart('/');
            string delFileGuid = project.FindFileGuidByProjectPath(projectPath);
            if (delFileGuid != null)
            {
                project.RemoveFileFromBuild(unityFrameworkTarget, delFileGuid);
                project.RemoveFile(delFileGuid);
            }
        }
        Directory.Delete(searchPath, true);
        string delFileGuid2 = project.FindFileGuidByProjectPath(Path.Combine(FRAMEWORK_TARGET_PATH, FRAMEWORK_NAME));
        if (delFileGuid2 != null)
        {
            project.RemoveFileFromBuild(unityFrameworkTarget, delFileGuid2);
            project.RemoveFile(delFileGuid2);
        }

        string delFileGuid3 = project.FindFileGuidByProjectPath(Path.Combine(FRAMEWORK_TARGET_PATH, XC_FRAMEWORK_NAME));
        if (delFileGuid3 != null)
        {
            project.RemoveFileFromBuild(unityFrameworkTarget, delFileGuid3);
            project.RemoveFile(delFileGuid3);
        }

        string frameworkDir = Path.Combine(pathToBuiltProject, FRAMEWORK_TARGET_PATH);
        Directory.Delete(frameworkDir, true);

        bool hasFramwork = project.ContainsFileByProjectPath(Path.Combine("Frameworks", XC_FRAMEWORK_NAME));

        //xcframeworkをコピー
        string srcPath = Path.Combine(XC_FRAMEWORK_SRC_DIR, XC_FRAMEWORK_NAME);
        string targetPath = Path.Combine(pathToBuiltProject, "Frameworks", XC_FRAMEWORK_NAME);

        CopyAndReplaceDirectory(srcPath, targetPath);

        if (hasFramwork)
        {
            return null;
        }
        //link追加
        string rfFrameworkGuid = project.AddFile(targetPath, Path.Combine("Frameworks", XC_FRAMEWORK_NAME));
        project.AddFileToEmbedFrameworks(mainTargetGuid, rfFrameworkGuid);

        return rfFrameworkGuid;

    }

    private static void configureMainTarget(PBXProject project, string mainTargetGuid, string pathToBuiltProject, string appGroupId)
    {

        var entitlementsPath = $"Unity-iPhone/Unity-iPhone.entitlements";
        if (project.AddCapability(mainTargetGuid, PBXCapabilityType.AppGroups, entitlementsPath))
        {
            Debug.Log("Added App Group Capability.");
        }

        if (project.AddCapability(mainTargetGuid, PBXCapabilityType.BackgroundModes, entitlementsPath))
        {
            Debug.Log("Added BackgroundModes Capability.");
        }

        if (project.AddCapability(mainTargetGuid, PBXCapabilityType.PushNotifications, entitlementsPath))
        {
            Debug.Log("Added PushNotifications Capability");
        }

        string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var capManager = new ProjectCapabilityManager(pbxProjectPath, entitlementsPath, null, mainTargetGuid);
        capManager.AddAppGroups(new string[] { appGroupId });
        Debug.Log($"Set App Group - {appGroupId}.");

        capManager.AddPushNotifications(Debug.isDebugBuild);
        Debug.Log("Set PushNotifications");

        capManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        Debug.Log("Set RemoteNotifications in BackgroundModes");

        capManager.WriteToFile();

        var entitlement = new PlistDocument();
        string entitlementAbsolutePath = Path.Combine(pathToBuiltProject, entitlementsPath);
        entitlement.ReadFromFile(entitlementAbsolutePath);

        PlistElementDict entitle = entitlement.root.AsDict();
        entitle.SetBoolean("com.apple.developer.usernotifications.time-sensitive", true);
        entitlement.WriteToFile(entitlementAbsolutePath);

        string infoPlistPath = Path.Combine(pathToBuiltProject, "Info.plist");

        RFProjectSettings settings= RFProjectSettings.GetOrCreate();
        string sdkKey = settings.m_sdkkey;
        int launchMode = settings.m_launchMode;
        bool sandbox = false;
        if (settings.m_environment == 0)
        {
            sandbox = Debug.isDebugBuild ? true : false;
        } else if (settings.m_environment == 1)
        {
            sandbox = true;
        } else if (settings.m_environment == 2)
        {
            sandbox = false;
        }

        var plist = new PlistDocument();
        plist.ReadFromFile(infoPlistPath);
        PlistElementDict dict = plist.root.CreateDict("RichFlyer");
        dict.SetString("serviceKey", sdkKey);
        dict.SetString("groupId", appGroupId);
        dict.SetBoolean("sandbox", sandbox);
        dict.SetInteger("launchMode", launchMode);
        plist.WriteToFile(infoPlistPath);

    }

    private static void configureUnityFramework(PBXProject project, string mainTargetGuid, string rfFrameworkGuid)
    {
        if (rfFrameworkGuid == null)
        {
            // xcframework has already added.
            return;
        }
        string unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
        string nseBuildPhaseFramework = project.GetFrameworksBuildPhaseByTarget(unityFrameworkTarget);
        project.AddFileToBuildSection(unityFrameworkTarget, nseBuildPhaseFramework, rfFrameworkGuid);
    }

    private static void addNotificationServiceFramework(PBXProject project, string mainTargetGuid,
        string pathToBuiltProject, string appGroupId, string rfFrameworkGuid)
    {

        string nseBundleId = PlayerSettings.applicationIdentifier + "." + NSE_NAME;
        string nseDstDir = Path.Combine(pathToBuiltProject, NSE_NAME);
        string nseSrcDir = Path.Combine(APPEXTENSION_SRC_DIR, NSE_NAME);
        if (!Directory.Exists(nseDstDir))
        {
            Directory.CreateDirectory(nseDstDir);
        }

        // create plist
        var notificationServicePlist = new PlistDocument();
        notificationServicePlist.ReadFromFile(Path.Combine(nseSrcDir, "Info.plist"));
        notificationServicePlist.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
        notificationServicePlist.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);
        // insert RichFlyer section to plist

        RFProjectSettings settings = RFProjectSettings.GetOrCreate();
        bool displayNavigate = settings.m_displayNavigate;

        PlistElementDict dict = notificationServicePlist.root.CreateDict("RichFlyer");
        dict.SetString("groupId", appGroupId);
        dict.SetBoolean("displayNavigate", displayNavigate);

        // save plist
        string nseInfoPlistPath = Path.Combine(nseDstDir, "Info.plist");
        notificationServicePlist.WriteToFile(nseInfoPlistPath);


        // copy extension source files
        string nseHeaderPath = Path.Combine(nseDstDir, NSE_HEADER_FILE);
        string nseSourcePath = Path.Combine(nseDstDir, NSE_SOURCE_FILE);
        File.Copy(Path.Combine(nseSrcDir, NSE_HEADER_FILE), nseHeaderPath, true);
        File.Copy(Path.Combine(nseSrcDir, NSE_SOURCE_FILE), nseSourcePath, true);

        if (rfFrameworkGuid == null)
        {
            // extension has aleady added.
            return;
        }

        // add extension
        string nseTarget = project.AddAppExtension(mainTargetGuid, NSE_NAME, nseBundleId, nseInfoPlistPath);
        string nseBuildPhaseFramework = project.GetFrameworksBuildPhaseByTarget(nseTarget);
        project.AddFileToBuildSection(nseTarget, nseBuildPhaseFramework, rfFrameworkGuid);

        project.AddFileToBuild(nseTarget, project.AddFile(nseHeaderPath, Path.Combine(NSE_NAME, NSE_HEADER_FILE)));
        project.AddFileToBuild(nseTarget, project.AddFile(nseSourcePath, Path.Combine(NSE_NAME, NSE_SOURCE_FILE)));
        project.AddFile(nseInfoPlistPath, Path.Combine(NSE_NAME, "Info.plist"));

        // add framework
        project.AddFrameworkToProject(nseTarget, "UserNotifications.framework", false);

        string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        project.WriteToFile(pbxProjectPath);

        var nseEntitlementsPath = Path.Combine(NSE_NAME, NSE_NAME + ".entitlements");

        // add capability
        var nseCapManager = new ProjectCapabilityManager(pbxProjectPath, nseEntitlementsPath, NSE_NAME);
        nseCapManager.AddAppGroups(new string[] { appGroupId });
        nseCapManager.WriteToFile();
        project.SetBuildProperty(nseTarget, "CODE_SIGN_ENTITLEMENTS", nseEntitlementsPath);

        Debug.Log($"Add Notification Service Extension.({nseBundleId})");

    }

    private static void addNotificationContentFramework(PBXProject project, string mainTargetGuid,
    string pathToBuiltProject, string appGroupId, string rfFrameworkGuid)
    {
        // initialize
        string nceBundleId = PlayerSettings.applicationIdentifier + "." + NCE_NAME;
        string nceDstDir = Path.Combine(pathToBuiltProject, NCE_NAME);
        string nceSrcDir = Path.Combine(APPEXTENSION_SRC_DIR, NCE_NAME);

        if (!Directory.Exists(nceDstDir))
        {
            Directory.CreateDirectory(nceDstDir);
        }

        // create plist
        string nceInfoPlistPath = Path.Combine(nceDstDir, "Info.plist");

        var notificationContentPlist = new PlistDocument();
        notificationContentPlist.ReadFromFile(Path.Combine(APPEXTENSION_SRC_DIR, NCE_NAME, "Info.plist"));
        notificationContentPlist.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
        notificationContentPlist.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);
        notificationContentPlist.WriteToFile(nceInfoPlistPath);

        // copy extension source files
        string nceHeaderPath = Path.Combine(nceDstDir, NCE_HEADER_FILE);
        string nceSourcePath = Path.Combine(nceDstDir, NCE_SOURCE_FILE);
        string nceBaseProjDir = Path.Combine(nceDstDir, NCE_BASEPROJ_DIR);
        if (!Directory.Exists(nceBaseProjDir))
        {
            Directory.CreateDirectory(nceBaseProjDir);
        }

        string nceStoryBoadPath = Path.Combine(nceBaseProjDir, NCE_STORYBORAD_FILE);
        File.Copy(Path.Combine(nceSrcDir, NCE_HEADER_FILE), nceHeaderPath, true);
        File.Copy(Path.Combine(nceSrcDir, NCE_SOURCE_FILE), nceSourcePath, true);
        File.Copy(Path.Combine(nceSrcDir, NCE_BASEPROJ_DIR, NCE_STORYBORAD_FILE), nceStoryBoadPath, true);

        if (rfFrameworkGuid == null)
        {
            // extension had already added.
            return;
        }

        // add extension
        string nceTarget = project.AddAppExtension(mainTargetGuid, NCE_NAME, nceBundleId, nceInfoPlistPath);
        string nceBuildPhaseFramework = project.GetFrameworksBuildPhaseByTarget(nceTarget);
        project.AddFileToBuildSection(nceTarget, nceBuildPhaseFramework, rfFrameworkGuid);

        project.AddFileToBuild(nceTarget, project.AddFile(nceHeaderPath, Path.Combine(NCE_NAME, NCE_HEADER_FILE)));
        project.AddFileToBuild(nceTarget, project.AddFile(nceSourcePath, Path.Combine(NCE_NAME, NCE_SOURCE_FILE)));
        project.AddFileToBuild(nceTarget, project.AddFile(nceStoryBoadPath, Path.Combine(NCE_NAME, NCE_STORYBORAD_FILE)));
        project.AddFile(nceInfoPlistPath, Path.Combine(NCE_NAME, "Info.plist"));

        project.AddFrameworkToProject(nceTarget, "UserNotifications.framework", false);
        project.AddFrameworkToProject(nceTarget, "UserNotificationsUI.framework", false);

        // save
        string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        project.WriteToFile(pbxProjectPath);

        var nceEntitlementsPath = Path.Combine(NCE_NAME, NCE_NAME + ".entitlements");

        // add capability
        var nceCapManager = new ProjectCapabilityManager(pbxProjectPath, nceEntitlementsPath, NCE_NAME);
        nceCapManager.AddAppGroups(new string[] { appGroupId });
        nceCapManager.WriteToFile();
        project.SetBuildProperty(nceTarget, "CODE_SIGN_ENTITLEMENTS", nceEntitlementsPath);

        Debug.Log($"Add Notification Content Extension.({nceBundleId})");
    }

    private static void removeUnusedFile(PBXProject project, string pathToBuiltProject)
    {
        string frameworkGuid = project.GetUnityFrameworkTargetGuid();
        string searchPath = Path.Combine(pathToBuiltProject, LIBRARIES_EXTENSION_PATH);
        string[] deleteFiles = Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories);
        foreach (string file in deleteFiles)
        {
            string projectPath = file.Replace(pathToBuiltProject, "").TrimStart('/');
            string delFileGuid = project.FindFileGuidByProjectPath(projectPath);
            if (delFileGuid != null)
            {
                project.RemoveFileFromBuild(frameworkGuid, delFileGuid);
                project.RemoveFile(delFileGuid);
            }
        }
    }
#endif

    private static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(dstPath))
        {
            Directory.Delete(dstPath, true);
        }

        if (File.Exists(dstPath))
        {
            File.Delete(dstPath);
        }

        Directory.CreateDirectory(dstPath);

        foreach (var file in Directory.GetFiles(srcPath))
        {
            if (Path.GetExtension(file).Equals(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
        }

        foreach (var dir in Directory.GetDirectories(srcPath))
        {
            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
        }
    }
}