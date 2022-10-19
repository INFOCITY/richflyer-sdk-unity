//
//  RichFlyer
//
//  Copyright © 2022年 INFOCITY,Inc. All rights reserved.
//

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RichFlyer
{
    public class RFProjectSettingsProvider : SettingsProvider
    {
        private const string SettingPath = "Project/RichFlyer";
        private SerializedObject m_settings;

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            // SettingsScopeをProjectにします
            return new RFProjectSettingsProvider(SettingPath, SettingsScope.Project, null);
        }

        public RFProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_settings = RFProjectSettings.GetSerializedObject();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_settings.FindProperty("m_sdkkey"), new GUIContent("SDK Key"));

            EditorGUILayout.Space();

            int launchMode = m_settings.FindProperty("m_launchMode").intValue;

            bool launchModeMovie = true;
            bool launchModeGif = true;
            bool launchModeImage = false;
            bool launchModeText = false;
            if (launchMode >= 0)
            {
                
                launchModeMovie = (launchMode & (int)RFLaunchModes.RFLaunchModeMovie) > 0;
                launchModeGif = (launchMode & (int)RFLaunchModes.RFLaunchModeGif) > 0;
                launchModeImage = (launchMode & (int)RFLaunchModes.RFLaunchModeImage) > 0;
                launchModeText = (launchMode & (int)RFLaunchModes.RFLaunchModeText) > 0;
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Launch Mode");
            launchModeMovie = GUILayout.Toggle(launchModeMovie, "Movie");
            launchModeGif = GUILayout.Toggle(launchModeGif, "Gif");
            launchModeImage = GUILayout.Toggle(launchModeImage, "Image");
            launchModeText = GUILayout.Toggle(launchModeText, "Text");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Select the type of notification to be displayed when the app is launched with the notification unopened.",
                MessageType.None);


            int selectedMode = 0;
            selectedMode |= launchModeMovie ? (int)RFLaunchModes.RFLaunchModeMovie : 0;
            selectedMode |= launchModeGif ? (int)RFLaunchModes.RFLaunchModeGif : 0;
            selectedMode |= launchModeImage ? (int)RFLaunchModes.RFLaunchModeImage : 0;
            selectedMode |= launchModeText ? (int)RFLaunchModes.RFLaunchModeText : 0;
            m_settings.FindProperty("m_launchMode").intValue = selectedMode;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.LabelField("iOS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_settings.FindProperty("m_groupId"), new GUIContent("App Group Id"));
            EditorGUILayout.HelpBox("If empty, the value is \"group.(BundleIdentifier)\".",
                MessageType.None);
            EditorGUILayout.Space();

            m_settings.FindProperty("m_displayNavigate").boolValue = EditorGUILayout.Toggle("Display Navigate", m_settings.FindProperty("m_displayNavigate").boolValue);
            EditorGUILayout.HelpBox("Check this box if you wish to display auxiliary text at the end of messages in the device's notification list.\nFor example, you can display \"Hold down to show detail\" to encourage users to read the full text.",
                MessageType.None);
            EditorGUILayout.Space();

            string[] environments = new string[] { "None", "Sandbox", "Production"};
            m_settings.FindProperty("m_environment").intValue = EditorGUILayout.Popup("Environment", m_settings.FindProperty("m_environment").intValue, environments);
            EditorGUILayout.HelpBox("If \"None\" is selected, it depends on the \"Development Build\" option in the Build Settings.\"On\" for Sandbox and \"Off\" for Production.",
                MessageType.None);
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_settings.FindProperty("m_themeColor"), new GUIContent("Theme Color"));
            EditorGUILayout.HelpBox("Theme color for the Notification Details dialog.Please enter a hexadecimal color code.Example: #2E8B57",
                MessageType.None);

            m_settings.ApplyModifiedProperties();
        }
    }

}
