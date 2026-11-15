using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using WakeTheWalls.Interaction;
using WakeTheWalls.Murals;
using WakeTheWalls.Rig;
using static WakeTheWalls.EditorTools.MuralAnimationSetup;

namespace WakeTheWalls.EditorTools
{
    // Keeps every mural flat on the wall, gives Mural 4 its face stories, lights the Mural 6 tower on tap,
    // rebuilds Mural 7 from the full photo and sets each mural's soundtrack. Safe to run more than once.
    public static class WallTapSetup
    {
        const string DataDir = "Assets/_Project/Murals/Data/";
        const string VoiceDir = "Assets/Art/Narrator/Voice/";

        static readonly string[] Faces = { "portrait_crown", "portrait_turban", "portrait_beret", "portrait_right", "portrait_cap" };

        [MenuItem("Wake the Walls/Apply Wall And Tap Update")]
        public static void Run()
        {
            AssetDatabase.Refresh();
            for (int i = 1; i <= 6; i++) Edit($"Mural{i}/Mural{i}Experience", Flatten);
            Edit("Mural4/Mural4Experience", Mural4);
            Edit("Mural6/Mural6Experience", Mural6);
            BuildMural7();
            Mural4Data();
            SetAmbience(7, "Assets/_Project/Audio/Music/Mural7_Enclosure.ogg");
            AssetDatabase.SaveAssets();
            Debug.Log("Wall and tap update applied.");
        }

        // Nothing comes off the wall any more except the Mural 2 flowers and the Mural 5 rings, which are their own 3D pieces.
        static void Flatten(GameObject root)
        {
            foreach (LayerParallax p in root.GetComponentsInChildren<LayerParallax>(true)) p.enabled = false;
            Remove<FlyOut>(root);
            Remove<TapJumpOut>(root);
            Remove<OrbitAround>(root);
            Remove<Butterflies>(root);
            Remove<PopOutIntro>(root);
        }

        static void Mural4(GameObject root)
        {
            var experience = root.GetComponent<Mural04Experience>();
            var so = new SerializedObject(experience);
            SerializedProperty portraits = so.FindProperty("portraits");
            portraits.arraySize = Faces.Length;
            for (int i = 0; i < Faces.Length; i++) portraits.GetArrayElementAtIndex(i).stringValue = Faces[i];
            Vector3[] rings =
            {
                new Vector3(0.62f, 0.788f, 0.096f), new Vector3(0.802f, 0.708f, 0.1f), new Vector3(0.781f, 0.498f, 0.066f),
                new Vector3(0.894f, 0.511f, 0.07f), new Vector3(0.818f, 0.234f, 0.092f),
            };
            SerializedProperty list = so.FindProperty("rings");
            list.arraySize = rings.Length;
            for (int i = 0; i < rings.Length; i++) list.GetArrayElementAtIndex(i).vector3Value = rings[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            // a slow wave of warm colour through the mask
            AssetImporterFor("Mural4/mural4_M05_mask_area");
            Sweep(root, "Mural4/mural4_M05_mask_area", width: 0.14f, direction: 0f, seconds: 3f, repeat: 6f, firstDelay: 2f,
                colour: new Color(1.25f, 0.85f, 0.5f, 1f));
        }

        static void Mural6(GameObject root)
        {
            var experience = root.GetComponent<LayeredMuralExperience>();
            AssetImporterFor("Mural6/mural6_M04_tower");
            Sweep(root, "Mural6/mural6_M04_tower", width: 0.12f, direction: 90f, seconds: 1.8f, repeat: 0f, firstDelay: 0f,
                colour: new Color(1.35f, 1.05f, 0.65f, 1f));
            var towerTex = AssetDatabase.LoadAssetAtPath<Texture>(ArtDir + "Mural6/mural6_M04_tower.png");
            MaskSweep tower = null, beams = null;
            foreach (MaskSweep sweep in root.GetComponents<MaskSweep>())
            {
                var mask = new SerializedObject(sweep).FindProperty("mask").objectReferenceValue;
                if (mask == towerTex) tower = sweep;
                else if (mask != null && mask.name.Contains("beams")) beams = sweep;
            }

            Transform old = root.transform.Find("TapTower");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var go = new GameObject("TapTower", typeof(BoxCollider), typeof(Interactable), typeof(SweepTapInteraction));
            go.transform.SetParent(root.transform, false);
            var tap = go.GetComponent<SweepTapInteraction>();
            var so = new SerializedObject(tap);
            so.FindProperty("experience").objectReferenceValue = experience;
            so.FindProperty("area").rectValue = new Rect(0.04f, 0.12f, 0.3f, 0.7f);
            SerializedProperty sweeps = so.FindProperty("sweeps");
            sweeps.arraySize = beams != null ? 2 : 1;
            sweeps.GetArrayElementAtIndex(0).objectReferenceValue = tower;
            if (beams != null) sweeps.GetArrayElementAtIndex(1).objectReferenceValue = beams;
            so.FindProperty("interactionId").stringValue = "tower";
            so.ApplyModifiedPropertiesWithoutUndo();
            var used = (UnityEngine.Events.UnityEvent<string>)typeof(SweepTapInteraction)
                .GetField("used", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tap);
            UnityEventTools.AddPersistentListener(used, experience.ReportInteraction);
        }

        static void Mural4Data()
        {
            var data = AssetDatabase.LoadAssetAtPath<MuralData>(DataDir + "Mural4.asset");
            var so = new SerializedObject(data);
            so.FindProperty("story").stringValue =
                "The Spirit of African Leadership, by Samuel Daddy Ishimwe and Masha Studio from Rwanda, painted in 2020.\n\n" +
                "A large mask of curved, interlocking shapes in gold, blue and black watches over five leaders painted into dark landscapes " +
                "and divided by lines of gold: Queen Nzinga, Mansa Musa, Thomas Sankara, Kwame Nkrumah and Winnie Madikizela-Mandela. " +
                "Mixing modern portraits with traditional mask symbolism, the mural links ancestral wisdom to the hopes of future generations, " +
                "and celebrates African leadership, innovation and cultural identity.\n\n" +
                "Tap a face to light it up and hear who it is, or tap the gold lines to let the light connect them.";
            string[,] spots =
            {
                { "portrait_crown", "Queen Nzinga", "She ruled the Ndongo and Matamba kingdoms in the 17th century, in what is now Angola. She is painted in royal regalia with a crown and necklace, after her best-known historical portrait.", "Mural4_face_crown" },
                { "portrait_turban", "Mansa Musa", "Emperor of the Mali Empire in the 14th century, often called one of the wealthiest people in history. He is painted in white robes and a headdress, holding a golden staff.", "Mural4_face_turban" },
                { "portrait_beret", "Thomas Sankara", "Former President of Burkina Faso and a Pan-Africanist revolutionary, painted in his signature red beret and military uniform.", "Mural4_face_beret" },
                { "portrait_right", "Kwame Nkrumah", "The first Prime Minister and President of Ghana, and a leading figure in the Pan-African movement. He sits next to Sankara.", "Mural4_face_right" },
                { "portrait_cap", "Winnie Madikizela-Mandela", "South African anti-apartheid activist and politician, painted smiling in glasses, a headwrap and beaded jewellery.", "Mural4_face_cap" },
            };
            SerializedProperty list = so.FindProperty("spots");
            list.arraySize = spots.GetLength(0);
            for (int i = 0; i < spots.GetLength(0); i++)
            {
                SerializedProperty s = list.GetArrayElementAtIndex(i);
                s.FindPropertyRelative("topic").stringValue = spots[i, 0];
                s.FindPropertyRelative("title").stringValue = spots[i, 1];
                s.FindPropertyRelative("text").stringValue = spots[i, 2];
                s.FindPropertyRelative("voice").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(VoiceDir + spots[i, 3] + ".mp3");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
        }

        static void SetAmbience(int mural, string clipPath)
        {
            var data = AssetDatabase.LoadAssetAtPath<MuralData>(DataDir + $"Mural{mural}.asset");
            var importer = AssetImporter.GetAtPath(clipPath) as AudioImporter;
            var like = AssetImporter.GetAtPath("Assets/_Project/Audio/Music/Mural6_Caravan.ogg") as AudioImporter;
            if (importer != null && like != null)
            {
                importer.defaultSampleSettings = like.defaultSampleSettings;
                importer.loadInBackground = like.loadInBackground;
                importer.forceToMono = like.forceToMono;
                importer.SaveAndReimport();
            }
            Set(data, "ambience", AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath));
            EditorUtility.SetDirty(data);
        }

        // New mask textures read like the other M files: single greyscale, no compression artefacts on the edge.
        static void AssetImporterFor(string texture)
        {
            var importer = AssetImporter.GetAtPath(ArtDir + texture + ".png") as TextureImporter;
            if (importer == null) return;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
        }
    }
}
