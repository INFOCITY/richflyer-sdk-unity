using System.IO;
using UnityEditor;

namespace jp.co.infocity.RichFlyer
{
    /// <summary>
    /// Removes assets that were included in older RichFlyer Unity packages but
    /// are no longer part of the current package.
    /// </summary>
    [InitializeOnLoad]
    internal static class RFLegacyAssetCleanup
    {
        private const string LegacyCodeRequirementsPath =
            "Assets/Plugins/iOS/RichFlyer/RichFlyer.xcframework/_CodeSignature/CodeRequirements-1";

        static RFLegacyAssetCleanup()
        {
            EditorApplication.delayCall += RemoveLegacyAssets;
        }

        private static void RemoveLegacyAssets()
        {
            if (!File.Exists(LegacyCodeRequirementsPath))
            {
                return;
            }

            File.Delete(LegacyCodeRequirementsPath);
            File.Delete(LegacyCodeRequirementsPath + ".meta");
            AssetDatabase.Refresh();
        }
    }
}
