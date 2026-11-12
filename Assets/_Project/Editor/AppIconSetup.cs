using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace WakeTheWalls.EditorTools
{
    // Sets Pearl's scan icon as the app icon for every platform.
    public static class AppIconSetup
    {
        const string IconPath = "Assets/Art/UI/AppIcon/wtw_app_icon.png";

        [MenuItem("Wake the Walls/Set App Icon")]
        public static void Run()
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            imp.textureType = TextureImporterType.Default;
            imp.mipmapEnabled = false;
            imp.alphaSource = TextureImporterAlphaSource.None;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.maxTextureSize = 1024;
            imp.SaveAndReimport();

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

            foreach (var target in new[] { NamedBuildTarget.iOS, NamedBuildTarget.Android })
            {
                int count = PlayerSettings.GetIconSizes(target, IconKind.Any).Length;
                PlayerSettings.SetIcons(target, Enumerable.Repeat(icon, count).ToArray(), IconKind.Any);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("App icon set.");
        }
    }
}
