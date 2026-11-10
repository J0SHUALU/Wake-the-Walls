using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WakeTheWalls.EditorTools
{
    public static class UIPreview
    {
        static readonly string[] Screens = { "StartScreen", "ScanScreen", "ARHud", "InfoPanel", "TrackingLostOverlay", "CompletionScreen", "HelpOverlay" };

        [MenuItem("Wake the Walls/Render UI Previews")]
        public static void Run()
        {
            Directory.CreateDirectory("UIPreviews");
            var camGo = new GameObject("PreviewCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.25f, 0.3f, 0.35f);
            var rt = new RenderTexture(585, 1266, 24);
            cam.targetTexture = rt;

            foreach (var name in Screens)
            {
                var canvasGo = new GameObject("PreviewCanvas", typeof(Canvas), typeof(CanvasScaler));
                var canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 1;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1170, 2532);
                scaler.matchWidthOrHeight = 0.5f;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/UI/{name}.prefab");
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasGo.transform);
                var r = (RectTransform)inst.transform;
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
                foreach (var g in inst.GetComponentsInChildren<CanvasGroup>(true)) { g.alpha = 1; g.interactable = true; }
                foreach (Transform t in inst.GetComponentsInChildren<Transform>(true)) t.gameObject.SetActive(true);

                Canvas.ForceUpdateCanvases();
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                File.WriteAllBytes($"UIPreviews/{name}.png", tex.EncodeToPNG());
                RenderTexture.active = null;
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(canvasGo);
            }
            Object.DestroyImmediate(camGo);
            rt.Release();
            Debug.Log("UI previews saved.");
        }
    }
}
