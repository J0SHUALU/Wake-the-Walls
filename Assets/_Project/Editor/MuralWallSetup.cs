using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WakeTheWalls.Murals;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.EditorTools
{
    public static class MuralWallSetup
    {
        const string PrefabDir = "Assets/_Project/Prefabs/";
        const string ArtDir = "Assets/Art/Murals/";
        const float MaxStepMm = 1f;

        static readonly string[] Mural2Flowers = { "flowers_left", "flower_crown", "bloom_purple", "bloom_red", "bloom_yellow", "bloom_teal", "bloom_pink" };

        [MenuItem("Wake the Walls/Keep Murals On The Wall")]
        public static void Run()
        {
            for (int n = 1; n <= 7; n++)
            {
                FlattenManifest(n);
                string path = $"{PrefabDir}Mural{n}/Mural{n}Experience.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                int mural = n;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Flatten(root, mural);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Keep murals on the wall: Mural{mural} failed: {e}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Keep murals on the wall: done.");
        }

        static void FlattenManifest(int n)
        {
            string path = $"{ArtDir}Mural{n}/mural{n}_layers.json";
            if (!File.Exists(path)) return;
            string text = File.ReadAllText(path);
            string flat = Regex.Replace(text, "(\"depthMm\"\\s*:\\s*)-?[0-9.]+", "${1}0.0");
            if (flat == text) return;
            File.WriteAllText(path, flat);
            AssetDatabase.ImportAsset(path);
        }

        static void Flatten(GameObject root, int mural)
        {
            foreach (LayerParallax parallax in root.GetComponentsInChildren<LayerParallax>(true))
            {
                Set(parallax, "maxOffset", 0f);
                parallax.enabled = false;
            }

            Remove<PopOutIntro>(root);
            if (mural != 2) Remove<OrbitAround>(root);

            foreach (TapJumpOut jump in root.GetComponentsInChildren<TapJumpOut>(true))
            {
                if (mural != 2) { Object.DestroyImmediate(jump, true); continue; }
                KeepFlowerLinks(jump);
            }

            foreach (FlyOut fly in root.GetComponentsInChildren<FlyOut>(true))
            {
                var so = new SerializedObject(fly);
                if (mural != 2) so.FindProperty("copyElements").arraySize = 0;
                so.FindProperty("sparkLift").floatValue = 0f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            foreach (Mural03Experience three in root.GetComponentsInChildren<Mural03Experience>(true))
            {
                Set(three, "peelMeters", 0f);
                Set(three, "floor", (Object)null);
            }
            Remove<DesertFloor>(root);

            foreach (Mural04Experience four in root.GetComponentsInChildren<Mural04Experience>(true))
                Set(four, "peelDistance", 0f);

            foreach (MaskGlow glow in root.GetComponentsInChildren<MaskGlow>(true))
            {
                var so = new SerializedObject(glow);
                SerializedProperty depth = so.FindProperty("depthMm");
                depth.floatValue = Mathf.Min(depth.floatValue, MaxStepMm);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            foreach (MaskSweep sweep in root.GetComponentsInChildren<MaskSweep>(true))
            {
                var so = new SerializedObject(sweep);
                SerializedProperty depth = so.FindProperty("depthMm");
                depth.floatValue = Mathf.Min(depth.floatValue, MaxStepMm);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void KeepFlowerLinks(TapJumpOut jump)
        {
            var so = new SerializedObject(jump);
            SerializedProperty links = so.FindProperty("links");
            for (int i = links.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty elements = links.GetArrayElementAtIndex(i).FindPropertyRelative("elements");
                bool flowers = elements.arraySize > 0;
                for (int k = 0; k < elements.arraySize; k++)
                    if (!Mural2Flowers.Contains(elements.GetArrayElementAtIndex(k).stringValue)) flowers = false;
                if (!flowers) links.DeleteArrayElementAtIndex(i);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Remove<T>(GameObject root) where T : Component
        {
            foreach (T c in root.GetComponentsInChildren<T>(true)) Object.DestroyImmediate(c, true);
        }

        static void Set(Object target, string field, object value)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"Keep murals on the wall: no field {field} on {target.name}"); return; }
            switch (value)
            {
                case float f: p.floatValue = f; break;
                case null: p.objectReferenceValue = null; break;
                case Object o: p.objectReferenceValue = o; break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
