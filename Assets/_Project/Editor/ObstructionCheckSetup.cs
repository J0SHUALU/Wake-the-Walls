using UnityEditor;
using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.EditorTools
{
    // Gives each mural the small readable copy of its painting that the obstruction check compares against.
    public static class ObstructionCheckSetup
    {
        [MenuItem("Wake the Walls/Set Up Obstruction Check")]
        public static void Run()
        {
            AssetDatabase.Refresh();
            for (int i = 1; i <= 7; i++)
            {
                string path = $"Assets/Art/Murals/Mural{i}/mural{i}_check.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) { Debug.LogWarning("Missing " + path); continue; }
                importer.textureType = TextureImporterType.Default;
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();

                var data = AssetDatabase.LoadAssetAtPath<MuralData>($"Assets/_Project/Murals/Data/Mural{i}.asset");
                MuralAnimationSetup.Set(data, "checkImage", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                EditorUtility.SetDirty(data);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Obstruction check set up.");
        }
    }
}
