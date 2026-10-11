using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace WakeTheWalls.EditorTools
{
    public static class AppIconSetup
    {
        const string Folder = "Assets/Art/AppIcon/";

        [MenuItem("Wake the Walls/Apply App Icon")]
        public static void Apply()
        {
            Texture2D full = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "app_icon.png");
            Texture2D background = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "app_icon_adaptive_bg.png");
            Texture2D foreground = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "app_icon_adaptive_fg.png");
            if (full == null || background == null || foreground == null)
            {
                Debug.LogError("App icon art is missing from " + Folder);
                return;
            }

            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { full }, IconKind.Any);
            SetAndroid(AndroidPlatformIconKind.Adaptive, background, foreground);
            AssetDatabase.SaveAssets();
            Debug.Log("Wake the Walls app icon applied to iOS and Android.");
        }

        static void SetAndroid(PlatformIconKind kind, Texture2D background, Texture2D foreground)
        {
            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            foreach (PlatformIcon icon in icons)
            {
                icon.SetTextures(background, foreground);
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
        }
    }
}
