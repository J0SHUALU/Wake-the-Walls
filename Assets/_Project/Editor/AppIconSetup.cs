using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace WakeTheWalls.EditorTools
{
    /// <summary>
    /// Puts the Wake the Walls app icon (the wall stamp) into every icon slot the build uses:
    /// the default icon, which iOS and older Android versions use, and the Android adaptive icon.
    /// Run it once from the menu after the icon art changes; the result is saved in Project Settings.
    /// </summary>
    public static class AppIconSetup
    {
        const string Folder = "Assets/Art/AppIcon/";

        /// <summary>Applies the icon art in Assets/Art/AppIcon to the player settings.</summary>
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
            // Adaptive icons take the background layer first, then the foreground layer.
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
