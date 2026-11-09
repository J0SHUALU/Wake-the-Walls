using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WakeTheWalls.Narrator;

namespace WakeTheWalls.EditorTools
{
    public static class NarratorSetup
    {
        const string ArtFolder = "Assets/Art/Narrator/Pearl/";
        const string VoiceFolder = "Assets/Art/Narrator/Voice/";
        const string MenuTrack = "Assets/_Project/Audio/Music/Menu_Theme.ogg";
        const string LinesPath = "Assets/_Project/Data/NarratorLines.asset";
        const string PrefabPath = "Assets/_Project/Prefabs/Narrator/Pearl.prefab";
        const string ScenePath = "Assets/_Project/Scenes/Main.unity";

        static readonly string[] Parts =
            { "glow", "fin_l", "fin_r", "body", "sockets", "eye_l", "eye_r", "eyes_blink", "lashes", "mouth", "shine" };

        [MenuItem("Wake the Walls/Set Up Narrator")]
        public static void Run()
        {
            foreach (string part in Parts) ImportSprite(ArtFolder + part + ".png");
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { VoiceFolder })) ImportVoice(AssetDatabase.GUIDToAssetPath(guid));
            ImportMusic(MenuTrack);

            NarratorLines lines = BuildLines();
            GameObject prefab = BuildPrefab(lines);
            if (prefab == null) return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (var old in Object.FindObjectsByType<WakeTheWalls.Narrator.Narrator>(FindObjectsSortMode.None)) Object.DestroyImmediate(old.gameObject);
            foreach (var old in Object.FindObjectsByType<MenuMusic>(FindObjectsSortMode.None)) Object.DestroyImmediate(old.gameObject);

            PrefabUtility.InstantiatePrefab(prefab, scene);

            var music = new GameObject("Menu Music");
            music.AddComponent<AudioSource>();
            var menu = music.AddComponent<MenuMusic>();
            var so = new SerializedObject(menu);
            so.FindProperty("clip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(MenuTrack);
            so.ApplyModifiedPropertiesWithoutUndo();
            SceneManager_Move(music, scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Narrator set up: Pearl and the menu music are in Main.");
        }

        static void SceneManager_Move(GameObject go, UnityEngine.SceneManagement.Scene scene) =>
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);

        static void ImportSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogError("Missing narrator art: " + path); return; }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 780f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 1024;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        static void ImportVoice(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) return;
            importer.forceToMono = true;
            importer.loadInBackground = true;
            var sample = importer.defaultSampleSettings;
            sample.loadType = AudioClipLoadType.DecompressOnLoad;
            sample.compressionFormat = AudioCompressionFormat.Vorbis;
            sample.quality = 0.7f;
            importer.defaultSampleSettings = sample;
            importer.SaveAndReimport();
        }

        static void ImportMusic(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) { Debug.LogError("Missing menu music: " + path); return; }
            importer.loadInBackground = true;
            var sample = importer.defaultSampleSettings;
            sample.loadType = AudioClipLoadType.Streaming;
            sample.compressionFormat = AudioCompressionFormat.Vorbis;
            importer.defaultSampleSettings = sample;
            importer.SaveAndReimport();
        }

        static AudioClip Clip(string clipName) => AssetDatabase.LoadAssetAtPath<AudioClip>(VoiceFolder + clipName + ".mp3");

        static NarratorLines BuildLines()
        {
            var lines = AssetDatabase.LoadAssetAtPath<NarratorLines>(LinesPath);
            if (lines == null)
            {
                lines = ScriptableObject.CreateInstance<NarratorLines>();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(LinesPath));
                AssetDatabase.CreateAsset(lines, LinesPath);
            }

            var murals = new List<NarratorLines.MuralLines>();
            for (int i = 1; i <= 5; i++)
            {
                murals.Add(new NarratorLines.MuralLines
                {
                    referenceImageName = "Mural" + i,
                    intro = Clip($"Mural{i}_intro"),
                    about = Clip($"Mural{i}_about"),
                    tryThis = Clip($"Mural{i}_try"),
                    done = Clip($"Mural{i}_done"),
                });
            }
            lines.Set(Clip("general_hello"), Clip("general_tapme"), murals);
            EditorUtility.SetDirty(lines);
            AssetDatabase.SaveAssets();
            return lines;
        }

        static GameObject BuildPrefab(NarratorLines lines)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PrefabPath));
            var root = new GameObject("Pearl");
            root.AddComponent<AudioSource>();
            var narrator = root.AddComponent<WakeTheWalls.Narrator.Narrator>();

            var so = new SerializedObject(narrator);
            so.FindProperty("lines").objectReferenceValue = lines;
            Set(so, "glow", "glow");
            Set(so, "finLeft", "fin_l");
            Set(so, "finRight", "fin_r");
            Set(so, "body", "body");
            Set(so, "sockets", "sockets");
            Set(so, "eyeLeft", "eye_l");
            Set(so, "eyeRight", "eye_r");
            Set(so, "eyesBlink", "eyes_blink");
            Set(so, "lashes", "lashes");
            Set(so, "mouth", "mouth");
            Set(so, "shine", "shine");
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void Set(SerializedObject so, string field, string part) =>
            so.FindProperty(field).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + part + ".png");
    }
}
