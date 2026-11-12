using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Recording;

namespace WakeTheWalls.EditorTools
{
    public static class RecordButtonSetup
    {
        const string PrefabPath = "Assets/_Project/Prefabs/UI/ARHud.prefab";
        const string ShapeDir = "Assets/Art/UI/Shapes/";
        const string FontDir = "Assets/Art/UI/Fonts/";
        const int Slot = 5;

        static readonly Color NavyDeep = new Color(0f, 0.1333f, 0.3098f, 1f);
        static readonly Color Red = new Color(0.8157f, 0.051f, 0.1765f, 1f);
        static readonly string[] OtherButtons = { "InfoButton", "SoundButton", "ReplayButton", "HelpButton" };

        [MenuItem("Wake the Walls/Add Record Button")]
        public static void Run()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Build(root);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("Record button added to the AR HUD.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Record button setup failed: " + e);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void Build(GameObject root)
        {
            var content = (RectTransform)root.transform.Find("Content");
            var replay = (RectTransform)content.Find("ReplayButton");
            if (content == null || replay == null) throw new System.Exception("missing Content or ReplayButton");

            var old = content.Find("Record");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var record = new GameObject("Record", typeof(RectTransform)).GetComponent<RectTransform>();
            record.SetParent(content, false);
            record.anchorMin = Vector2.zero;
            record.anchorMax = Vector2.one;
            record.offsetMin = Vector2.zero;
            record.offsetMax = Vector2.zero;

            var buttonObject = Object.Instantiate(replay.gameObject, record);
            buttonObject.name = "RecordButton";
            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.anchoredPosition = new Vector2(-60, -27 - 144 * Slot);
            var button = buttonObject.GetComponent<Button>();
            var background = buttonObject.GetComponent<Image>();
            var icon = buttonRect.Find("Icon").GetComponent<Image>();
            var disc = Sprite("summit_disc");
            var square = Sprite("summit_square");
            icon.sprite = disc;
            icon.color = Red;

            var timer = Panel(buttonRect, "Timer", new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(330, 78), Red);
            var timerLabel = Label(timer, "Label", Font("IBMPlexMono-Medium"), 28.5f, 12.6f, TextAlignmentOptions.Center);
            timerLabel.text = "REC 00:00";

            var message = Panel(buttonRect, "Message", new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(-24, -63), new Vector2(660, 132), NavyDeep);
            var messageLabel = Label(message, "Label", Font("InstrumentSans-SemiBold"), 36f, 0f, TextAlignmentOptions.MidlineLeft);
            messageLabel.margin = new Vector4(36, 18, 36, 18);
            messageLabel.textWrappingMode = TextWrappingModes.Normal;

            var recorder = record.gameObject.AddComponent<ReplayKitRecorder>();
            var recordButton = record.gameObject.AddComponent<RecordButton>();
            var focus = record.gameObject.AddComponent<RecordingFocus>();

            Wire(recordButton, so =>
            {
                so.FindProperty("recorder").objectReferenceValue = recorder;
                so.FindProperty("controls").objectReferenceValue = buttonObject;
                so.FindProperty("button").objectReferenceValue = button;
                so.FindProperty("background").objectReferenceValue = background;
                so.FindProperty("icon").objectReferenceValue = icon;
                so.FindProperty("idleSprite").objectReferenceValue = disc;
                so.FindProperty("recordingSprite").objectReferenceValue = square;
                so.FindProperty("timerGroup").objectReferenceValue = timer.GetComponent<CanvasGroup>();
                so.FindProperty("timerLabel").objectReferenceValue = timerLabel;
                so.FindProperty("messageGroup").objectReferenceValue = message.GetComponent<CanvasGroup>();
                so.FindProperty("messageLabel").objectReferenceValue = messageLabel;
            });

            var hidden = OtherGroups(content);
            Wire(focus, so =>
            {
                so.FindProperty("recorder").objectReferenceValue = recorder;
                var list = so.FindProperty("hiddenWhileRecording");
                list.arraySize = hidden.Length;
                for (int i = 0; i < hidden.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = hidden[i];
            });
        }

        static CanvasGroup[] OtherGroups(RectTransform content)
        {
            var groups = new CanvasGroup[OtherButtons.Length];
            for (int i = 0; i < OtherButtons.Length; i++)
            {
                var t = content.Find(OtherButtons[i]);
                if (t == null) throw new System.Exception("missing " + OtherButtons[i]);
                groups[i] = t.GetComponent<CanvasGroup>() ?? t.gameObject.AddComponent<CanvasGroup>();
            }
            return groups;
        }

        static RectTransform Panel(RectTransform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color fill)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = null;
            image.color = fill;
            image.raycastTarget = false;
            go.GetComponent<CanvasGroup>().alpha = 0f;
            return rect;
        }

        static TMP_Text Label(RectTransform parent, string name, TMP_FontAsset font, float size, float spacing, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.characterSpacing = spacing;
            text.color = Color.white;
            text.alignment = align;
            text.raycastTarget = false;
            return text;
        }

        static void Wire(Object target, System.Action<SerializedObject> assign)
        {
            var so = new SerializedObject(target);
            assign(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(ShapeDir + name + ".png");

        static TMP_FontAsset Font(string file) => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir + file + " SDF.asset");
    }
}
