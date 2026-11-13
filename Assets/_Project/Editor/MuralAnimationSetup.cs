using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using WakeTheWalls.Interaction;
using WakeTheWalls.Murals;
using WakeTheWalls.Rig;

namespace WakeTheWalls.EditorTools
{
    // Applies the animation update to the mural prefabs. Safe to run more than once.
    public static class MuralAnimationSetup
    {
        const string PrefabDir = "Assets/_Project/Prefabs/";
        const string ArtDir = "Assets/Art/Murals/";

        [MenuItem("Wake the Walls/Apply Animation Update")]
        public static void Run()
        {
            Edit("Mural1/Mural1Experience", Mural1);
            Edit("Mural2/Mural2Experience", Mural2);
            Edit("Mural3/Mural3Experience", Mural3);
            Edit("Mural4/Mural4Experience", Mural4);
            Edit("Mural5/Mural5Experience", Mural5);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "Mural6/Mural6Experience.prefab") != null)
            {
                AddTexture(null, "Mural6/mural6_L05_riders", "Mural6/mural6_L01_caravan");
                Edit("Mural6/Mural6Experience", Mural6);
                BuildMural7();
            }
            else Debug.LogWarning("Animation update: merge Remy's murals 6 and 7 first, then run this again for them.");
            AssetDatabase.SaveAssets();
            Debug.Log("Animation update applied.");
        }

        static void Mural1(GameObject root)
        {
            AddTexture(root, "Mural1/mural1_L16_eye_sockets", "Mural1/mural1_L05_eye_left");
            Remove<PopOutIntro>(root);

            Motion(root, "earring_left", 7f, 0.38f);
            Motion(root, "earring_right", 7f, 0.38f);

            var eyes = root.GetComponentInChildren<EyesFollow>(true);
            Set(eyes, "maxOffset", 0.06f);
            Set(eyes, "followSpeed", 5f);

            Spin(root, ("robe_rosette", 22f));

            var sweep = root.GetComponent<MaskSweep>();
            if (sweep != null) Set(sweep, "repeatEvery", 4f);
        }

        // Flowers that come out stay, but they no longer flip as they fly.
        static void Mural2(GameObject root)
        {
            Remove<PopOutIntro>(root);
            foreach (FlyOut fly in root.GetComponentsInChildren<FlyOut>(true)) Set(fly, "spinDegrees", 0f);
            foreach (TapJumpOut jump in root.GetComponentsInChildren<TapJumpOut>(true)) Set(jump, "spinDegrees", 0f);

            for (int i = 0; i < Blooms.Length; i++) Motion(root, Blooms[i], 8f, 0.32f + i * 0.03f, 0.002f, i * 0.19f);
            Motion(root, "hand_left", 2.5f, 0.2f);
            Motion(root, "hand_right", 2.5f, 0.21f);

            Sweep(root, "Mural2/mural2_M01_collar_beads", repeat: 5f);
            Sweep(root, "Mural2/mural2_M02_africa_map", repeat: 7f);
            Sweep(root, "Mural2/mural2_M03_gold_lines", width: 0.05f, direction: 0f, seconds: 2f, repeat: 6f, firstDelay: 2f);
            Sweep(root, "Mural2/mural2_M04_water", width: 0.04f, direction: 0f, seconds: 2.5f, repeat: 3.5f, firstDelay: 1f,
                colour: new Color(1.2f, 1.25f, 1.3f, 0.8f));
            Glow(root, "Mural2/mural2_M06_windows", new Color(1f, 0.72f, 0.35f, 1f), 0.1f, 0.75f, 4f);
        }

        static readonly string[] Blooms = { "bloom_purple", "bloom_red", "bloom_yellow", "bloom_teal", "bloom_pink" };

        static void Mural3(GameObject root)
        {
            Remove<PopOutIntro>(root);
            Remove<TapJumpOut>(root);

            Motion(root, "agave_big", 3f, 0.24f);
            Motion(root, "agave_right", 3.5f, 0.27f);
            Motion(root, "plants_left", 3f, 0.3f);
            Motion(root, "cactus", 1.4f, 0.18f);
            Motion(root, "cactus_small", 2f, 0.22f);
            Drift(root, "clouds", 0.07f, 26f);

            Sweep(root, "Mural3/mural3_M01_domes", width: 0.06f, direction: 0f, seconds: 2.2f, repeat: 5f, firstDelay: 2f,
                colour: new Color(1.3f, 1.25f, 1.1f, 1f));
        }

        static void Mural4(GameObject root)
        {
            Remove<PopOutIntro>(root);
            foreach (MaskSweep sweep in root.GetComponentsInChildren<MaskSweep>(true)) Set(sweep, "repeatEvery", 5f);

            Sweep(root, "Mural4/mural4_M03_crown_sceptre", width: 0.03f, direction: -90f, seconds: 1.2f, repeat: 4f, firstDelay: 3f);
            Sweep(root, "Mural4/mural4_M04_stream", width: 0.04f, direction: 0f, seconds: 1.8f, repeat: 2.5f, firstDelay: 1f,
                colour: new Color(1.1f, 1.3f, 1.4f, 0.9f));
        }

        // The rings keep lifting out. Cubes and flowers now move in place instead of popping or flying off.
        static void Mural5(GameObject root)
        {
            Remove<PopOutIntro>(root);
            Remove<TapJumpOut>(root);
            Remove<OrbitAround>(root);
            foreach (FlyOut fly in root.GetComponentsInChildren<FlyOut>(true))
            {
                var so = new SerializedObject(fly);
                so.FindProperty("copyElements").arraySize = 0;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Motion(root, "earring", 9f, 0.4f);
            for (int i = 1; i <= 7; i++) Motion(root, "cube_" + i, 10f, 0.12f + i * 0.015f, 0.01f, i * 0.13f);
            Spin(root, ("flower_1", 40f), ("flower_2", -34f), ("flower_3", 30f), ("flower_4", -42f), ("flower_5", 36f));
            foreach (string flower in new[] { "flower_1", "flower_2", "flower_3", "flower_4", "flower_5" }) Motion(root, flower, 0f, 0.2f, 0.004f);
        }

        // The caravans walk one way to the right and fade out at the end of each trip, the riders bob along,
        // the oasis shimmers and light climbs the tower beams. No pop-outs or flips.
        static void Mural6(GameObject root)
        {
            AddTexture(root, "Mural6/mural6_L05_riders", "Mural6/mural6_L01_caravan");
            Remove<PopOutIntro>(root);
            Remove<TapJumpOut>(root);
            foreach (FlyOut fly in root.GetComponentsInChildren<FlyOut>(true)) Set(fly, "spinDegrees", 0f);

            var experience = root.GetComponent<LayeredMuralExperience>();
            Set(experience, "walkOneWay", true);
            Set(experience, "walkMeters", 0.3f);
            Set(experience, "walkSpeed", 0.05f);
            Set(experience, "stepMeters", 0.005f);
            Set(experience, "stepsPerSecond", 1.8f);

            Motion(root, "riders", 1.2f, 0.4f, 0.006f, 0.3f);
            Motion(root, "palms", 2.5f, 0.3f);

            foreach (MaskSweep sweep in root.GetComponents<MaskSweep>())
            {
                if (new SerializedObject(sweep).FindProperty("mask").objectReferenceValue?.name != "mural6_M02_beams") continue;
                Set(sweep, "direction", 90f);
                Set(sweep, "repeatEvery", 5f);
            }
            Dusk(root, false);

            Sweep(root, "Mural6/mural6_M03_water", width: 0.04f, direction: 0f, seconds: 1.8f, repeat: 3f, firstDelay: 2f,
                colour: new Color(1.2f, 1.3f, 1.4f, 0.9f));
        }

        // Mural 7 had no experience yet, so this builds one: the wall rebuilds brick by brick and light runs along it,
        // smoke curls up from the huts, the doorways flicker with firelight and birds circle the tower.
        static void BuildMural7()
        {
            const string path = PrefabDir + "Mural7/Mural7Experience.prefab";
            if (!AssetDatabase.IsValidFolder(PrefabDir + "Mural7")) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Mural7");

            var brickMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Shaders/BrickBuild.mat");
            if (brickMat == null)
            {
                brickMat = new Material(Shader.Find("WakeTheWalls/BrickBuild")) { name = "BrickBuild" };
                AssetDatabase.CreateAsset(brickMat, "Assets/_Project/Shaders/BrickBuild.mat");
            }
            foreach (string t in new[] { "mural7_L05_wall", "mural7_L06_wall_base" }) AddTexture(null, "Mural7/" + t, "Mural7/mural7_L01_hut_front");

            var six = PrefabUtility.LoadPrefabContents(PrefabDir + "Mural6/Mural6Experience.prefab");
            var sixRig = six.GetComponentInChildren<LayeredMuralRig>(true);
            var layerMaterial = new SerializedObject(sixRig).FindProperty("layerMaterial").objectReferenceValue;
            var sixMarkers = new SerializedObject(six.GetComponent<TapMarkers>());
            var ring = sixMarkers.FindProperty("ring").objectReferenceValue;
            var dot = sixMarkers.FindProperty("dot").objectReferenceValue;
            var burst = new SerializedObject(six.GetComponentInChildren<PulseInteraction>(true)).FindProperty("burstPrefab").objectReferenceValue;
            PrefabUtility.UnloadPrefabContents(six);

            var root = new GameObject("Mural7Experience");
            var rigGo = new GameObject("Rig");
            rigGo.transform.SetParent(root.transform, false);
            var rig = rigGo.AddComponent<LayeredMuralRig>();
            var rigSo = new SerializedObject(rig);
            rigSo.FindProperty("manifest").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(ArtDir + "Mural7/mural7_layers.json");
            rigSo.FindProperty("layerMaterial").objectReferenceValue = layerMaterial;
            var textures = new List<Texture>();
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtDir + "Mural7" }))
            {
                string file = AssetDatabase.GUIDToAssetPath(guid);
                string name = System.IO.Path.GetFileNameWithoutExtension(file);
                if (name.Contains("_L") || name.EndsWith("bg_filled")) textures.Add(AssetDatabase.LoadAssetAtPath<Texture>(file));
            }
            SerializedProperty list = rigSo.FindProperty("textures");
            list.arraySize = textures.Count;
            for (int i = 0; i < textures.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = textures[i];
            rigSo.ApplyModifiedPropertiesWithoutUndo();
            var parallax = rigGo.AddComponent<LayerParallax>();

            var motion = root.AddComponent<LayerMotionSet>();
            var experience = root.AddComponent<Mural07Experience>();
            Set(experience, "rig", rig);
            Set(experience, "motionSet", motion);
            Set(experience, "parallax", parallax);
            Set(experience, "requiredInteractions", 2);

            var markers = root.AddComponent<TapMarkers>();
            Set(markers, "experience", experience);
            Set(markers, "ring", ring);
            Set(markers, "dot", dot);
            Set(root.AddComponent<PaintInIntro>(), "experience", experience);

            Motion(root, "tower", 0f, 0.1f, 0.0015f, 0.1f);
            Motion(root, "hut_front", 0f, 0.1f, 0.0015f, 0.4f);
            Motion(root, "hut_right", 0f, 0.1f, 0.0015f, 0.7f);
            Motion(root, "huts_left", 0f, 0.1f, 0.0015f, 0.2f);

            Sweep(root, "Mural7/mural7_M02_wall", width: 0.06f, direction: 0f, seconds: 2.4f, repeat: 9f, firstDelay: 9f,
                colour: new Color(1.35f, 1.2f, 0.95f, 1f));
            MaskSweep wallLight = null;
            foreach (MaskSweep sweep in root.GetComponents<MaskSweep>()) wallLight = sweep;

            var build = root.AddComponent<BrickBuild>();
            Set(build, "experience", experience);
            Set(build, "element", "wall");
            Set(build, "brickMaterial", brickMat);
            Set(build, "afterBuild", wallLight);
            Set(experience, "wall", build);

            Glow(root, "Mural7/mural7_M01_doors", new Color(1f, 0.6f, 0.25f, 1f), 0.25f, 0.85f, 0.7f);

            var smoke = root.AddComponent<SmokePlume>();
            Set(smoke, "experience", experience);
            var smokeSo = new SerializedObject(smoke);
            Vector2[] roofs = { new Vector2(0.417f, 0.62f), new Vector2(0.615f, 0.69f), new Vector2(0.185f, 0.62f) };
            SerializedProperty sources = smokeSo.FindProperty("sources");
            sources.arraySize = roofs.Length;
            for (int i = 0; i < roofs.Length; i++) sources.GetArrayElementAtIndex(i).vector2Value = roofs[i];
            smokeSo.ApplyModifiedPropertiesWithoutUndo();

            Dusk(root, true);

            var birds = root.AddComponent<CirclingBirds>();
            Set(birds, "experience", experience);
            Set(birds, "centre", new Vector2(0.852f, 0.58f));

            Tap(root, experience, burst, "TapWall", new Rect(0f, 0f, 1f, 0.3f), new string[0], "wall", build);
            Tap(root, experience, burst, "TapTower", new Rect(0.76f, 0.2f, 0.2f, 0.3f), new[] { "tower" }, "tower", null);
            Tap(root, experience, burst, "TapHuts", new Rect(0.1f, 0.3f, 0.6f, 0.4f), new[] { "hut_front", "hut_right", "huts_left" }, "huts", null);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            var data = AssetDatabase.LoadAssetAtPath<MuralData>("Assets/_Project/Murals/Data/Mural7.asset");
            if (data != null) Set(data, "experiencePrefab", AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<MuralExperience>());
        }

        static void Dusk(GameObject root, bool embers)
        {
            var dusk = root.GetComponent<DuskCycle>();
            if (dusk == null) dusk = root.AddComponent<DuskCycle>();
            Set(dusk, "experience", root.GetComponent<LayeredMuralExperience>());
            Set(dusk, "embers", embers);
        }

        static void Tap(GameObject root, LayeredMuralExperience experience, Object burst, string name, Rect area, string[] elements, string id, BrickBuild rebuild)
        {
            var go = new GameObject(name, typeof(BoxCollider), typeof(Interactable), typeof(PulseInteraction));
            go.transform.SetParent(root.transform, false);
            var pulse = go.GetComponent<PulseInteraction>();
            var so = new SerializedObject(pulse);
            so.FindProperty("experience").objectReferenceValue = experience;
            so.FindProperty("area").rectValue = area;
            SerializedProperty els = so.FindProperty("elements");
            els.arraySize = elements.Length;
            for (int i = 0; i < elements.Length; i++) els.GetArrayElementAtIndex(i).stringValue = elements[i];
            so.FindProperty("burstPrefab").objectReferenceValue = burst;
            so.FindProperty("interactionId").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();

            var used = (UnityEngine.Events.UnityEvent<string>)typeof(PulseInteraction)
                .GetField("used", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pulse);
            UnityEventTools.AddPersistentListener(used, experience.ReportInteraction);
            if (rebuild != null) UnityEventTools.AddVoidPersistentListener(go.GetComponent<Interactable>().Tapped, rebuild.Rebuild);
        }

        // ---------- helpers ----------

        static void Sweep(GameObject root, string mask, float width = 0.04f, float direction = -90f, float seconds = 1.4f,
            float repeat = 6f, float firstDelay = 3f, Color? colour = null)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture>(ArtDir + mask + ".png");
            MaskSweep sweep = null;
            foreach (MaskSweep existing in root.GetComponents<MaskSweep>())
                if (new SerializedObject(existing).FindProperty("mask").objectReferenceValue == tex) sweep = existing;

            bool fresh = sweep == null;
            if (fresh)
            {
                sweep = root.AddComponent<MaskSweep>();
                Set(sweep, "experience", root.GetComponent<LayeredMuralExperience>());
                Set(sweep, "mask", tex);
                Set(sweep, "glowMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Shaders/MaskSweep.mat"));
                Set(sweep, "region", new Rect(0f, 0f, 1f, 1f));
                Set(sweep, "width", width);
                Set(sweep, "direction", direction);
                Set(sweep, "seconds", seconds);
                Set(sweep, "colour", colour ?? new Color(1.4f, 1.15f, 0.55f, 1f));
            }
            Set(sweep, "repeatEvery", repeat);
            Set(sweep, "firstDelay", firstDelay);
        }

        static void Glow(GameObject root, string mask, Color colour, float low, float high, float seconds)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture>(ArtDir + mask + ".png");
            string name = "Glow " + System.IO.Path.GetFileName(mask);
            Transform holder = root.transform.Find(name);
            if (holder == null)
            {
                holder = new GameObject(name).transform;
                holder.SetParent(root.transform, false);
            }
            var glow = holder.GetComponent<VFX.MaskGlow>();
            if (glow == null) glow = holder.gameObject.AddComponent<VFX.MaskGlow>();
            Set(glow, "mask", tex);
            Set(glow, "glowMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Shaders/MaskGlow.mat"));
            Set(glow, "colour", colour);
            Set(glow, "depthMm", 1.4f);

            var pulse = holder.GetComponent<VFX.GlowPulse>();
            if (pulse == null) pulse = holder.gameObject.AddComponent<VFX.GlowPulse>();
            Set(pulse, "experience", root.GetComponent<LayeredMuralExperience>());
            Set(pulse, "glow", glow);
            Set(pulse, "low", low);
            Set(pulse, "high", high);
            Set(pulse, "breathSeconds", seconds);
        }

        static void Drift(GameObject root, string element, float distance, float seconds)
        {
            LayerDrift drift = null;
            foreach (LayerDrift existing in root.GetComponents<LayerDrift>())
                if (new SerializedObject(existing).FindProperty("element").stringValue == element) drift = existing;
            if (drift == null) drift = root.AddComponent<LayerDrift>();
            Set(drift, "experience", root.GetComponent<LayeredMuralExperience>());
            Set(drift, "element", element);
            Set(drift, "distance", distance);
            Set(drift, "seconds", seconds);
        }

        // ---------- plumbing ----------

        static void Edit(string prefab, System.Action<GameObject> change)
        {
            string path = PrefabDir + prefab + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                change(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Animation update: {prefab} failed: {e}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void AddTexture(GameObject root, string texture, string copySettingsFrom)
        {
            string path = ArtDir + texture + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var like = (TextureImporter)AssetImporter.GetAtPath(ArtDir + copySettingsFrom + ".png");
            if (importer != null && like != null)
            {
                EditorUtility.CopySerialized(like, importer);
                importer.SaveAndReimport();
            }

            if (root == null) return;
            var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            var rig = root.GetComponentInChildren<LayeredMuralRig>(true);
            var so = new SerializedObject(rig);
            SerializedProperty list = so.FindProperty("textures");
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == tex) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = tex;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Remove<T>(GameObject root) where T : Component
        {
            foreach (T c in root.GetComponentsInChildren<T>(true)) Object.DestroyImmediate(c, true);
        }

        static void Motion(GameObject root, string element, float swayDegrees, float swaySpeed, float bobMeters = -1f, float phase = -1f)
        {
            var set = root.GetComponentInChildren<LayerMotionSet>(true);
            var so = new SerializedObject(set);
            SerializedProperty entries = so.FindProperty("entries");
            SerializedProperty entry = null;
            for (int i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("element").stringValue == element)
                    entry = entries.GetArrayElementAtIndex(i);
            if (entry == null)
            {
                entries.arraySize++;
                entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                entry.FindPropertyRelative("element").stringValue = element;
                entry.FindPropertyRelative("bobMeters").floatValue = 0f;
                entry.FindPropertyRelative("bobSpeed").floatValue = 0.3f;
                entry.FindPropertyRelative("phase").floatValue = (entries.arraySize * 0.37f) % 1f;
            }
            entry.FindPropertyRelative("swayDegrees").floatValue = swayDegrees;
            entry.FindPropertyRelative("swaySpeed").floatValue = swaySpeed;
            if (bobMeters >= 0f) entry.FindPropertyRelative("bobMeters").floatValue = bobMeters;
            if (phase >= 0f) entry.FindPropertyRelative("phase").floatValue = phase;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Spin(GameObject root, params (string element, float speed)[] items)
        {
            var experience = root.GetComponent<LayeredMuralExperience>();
            var spin = root.GetComponent<LayerSpin>();
            if (spin == null) spin = root.AddComponent<LayerSpin>();
            var so = new SerializedObject(spin);
            so.FindProperty("experience").objectReferenceValue = experience;
            SerializedProperty list = so.FindProperty("items");
            list.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("element").stringValue = items[i].element;
                list.GetArrayElementAtIndex(i).FindPropertyRelative("speed").floatValue = items[i].speed;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Set(Object target, string field, object value)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"Animation update: no field {field} on {target.name}"); return; }
            switch (value)
            {
                case float f: p.floatValue = f; break;
                case int i: p.intValue = i; break;
                case bool b: p.boolValue = b; break;
                case string s: p.stringValue = s; break;
                case Vector2 v: p.vector2Value = v; break;
                case Vector3 v3: p.vector3Value = v3; break;
                case Color c: p.colorValue = c; break;
                case Rect r: p.rectValue = r; break;
                case Object o: p.objectReferenceValue = o; break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
