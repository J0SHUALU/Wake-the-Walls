using UnityEditor;
using UnityEngine;

namespace WakeTheWalls.EditorTools
{
    // Every mural is loaded when the app starts, and each layer is a full-size canvas, so with seven murals
    // the phone ran out of memory. This caps the layer and mask textures on phones and turns off mipmaps.
    public static class MuralTextureSettings
    {
        const string ArtDir = "Assets/Art/Murals";
        const int LayerSize = 1024;
        const int BackgroundSize = 2048;

        [MenuItem("Wake the Walls/Fit Mural Textures For Phones")]
        public static void Run()
        {
            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.Contains("reference")) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                int size = name.EndsWith("bg_filled") ? BackgroundSize : LayerSize;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                foreach (string platform in new[] { "iPhone", "Android" })
                {
                    TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
                    settings.overridden = true;
                    settings.maxTextureSize = size;
                    settings.format = TextureImporterFormat.ASTC_6x6;
                    settings.textureCompression = TextureImporterCompression.Compressed;
                    importer.SetPlatformTextureSettings(settings);
                }
                importer.SaveAndReimport();
                changed++;
            }
            Debug.Log($"Mural textures fitted for phones: {changed}");
        }
    }
}
