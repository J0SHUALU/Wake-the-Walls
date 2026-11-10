using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Audio;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;
using WakeTheWalls.UI;

namespace WakeTheWalls.Narrator
{
    [RequireComponent(typeof(AudioSource))]
    public class Narrator : MonoBehaviour
    {
        const float CanvasWidth = 600f;
        const float CanvasHeight = 780f;

        [SerializeField] NarratorLines lines;

        [Header("Parts (600 x 780 canvas, centre pivot)")]
        [SerializeField] Sprite glow;
        [SerializeField] Sprite finLeft;
        [SerializeField] Sprite finRight;
        [SerializeField] Sprite body;
        [SerializeField] Sprite sockets;
        [SerializeField] Sprite eyeLeft;
        [SerializeField] Sprite eyeRight;
        [SerializeField] Sprite eyesBlink;
        [SerializeField] Sprite lashes;
        [SerializeField] Sprite mouth;
        [SerializeField] Sprite shine;

        [Header("Layout")]
        [Tooltip("Name of the HUD button Pearl's button sits under.")]
        [SerializeField] string anchorButton = "HelpButton";
        [SerializeField] float spacing = 144f;
        [Tooltip("Pearl's height on screen, in canvas units.")]
        [SerializeField] float pearlHeight = 520f;
        [Tooltip("Gap between the bottom of the screen and Pearl.")]
        [SerializeField] float bottomMargin = 140f;
        [Tooltip("Seconds Pearl waits after her last line before she floats away.")]
        [SerializeField] float idleBeforeHide = 4f;

        [Header("Voice")]
        [SerializeField] float gapBetweenLines = 0.35f;
        [SerializeField] float mouthGain = 9f;

        AudioSource voice;
        RectTransform button;
        Image buttonRing;
        Rig icon;
        Rig pearl;
        RectTransform pearlRoot;

        MuralData current;
        readonly Queue<AudioClip> queue = new Queue<AudioClip>();
        readonly HashSet<MuralData> introduced = new HashSet<MuralData>();
        readonly float[] samples = new float[256];
        bool greeted;
        bool active;
        bool shown;
        bool hasNews;
        int tapIndex;
        float level;
        float nextLineAt;
        float quietSince;
        Coroutine popping;

        class Rig
        {
            public RectTransform root, mouth, finL, finR;
            public GameObject eyesOpen, eyesClosed;
            public float nextBlinkAt, blinkUntil;
        }

        public bool IsSpeaking => voice != null && voice.isPlaying;

        void Awake()
        {
            voice = GetComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0f;
        }

        void Start() => Build();

        void OnEnable()
        {
            GameEvents.MuralFound += HandleFound;
            GameEvents.MuralLost += HandleLost;
            GameEvents.MuralCompleted += HandleCompleted;
            GameEvents.StateChanged += HandleState;
            GameEvents.ResetRequested += HandleReset;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleFound;
            GameEvents.MuralLost -= HandleLost;
            GameEvents.MuralCompleted -= HandleCompleted;
            GameEvents.StateChanged -= HandleState;
            GameEvents.ResetRequested -= HandleReset;
            AudioManager.SetDucked(false);
        }

        void ButtonPressed()
        {
            if (!active || lines == null) return;
            if (shown)
            {
                Silence();
                Hide();
                return;
            }

            Show();
            hasNews = false;
            var mural = lines.For(current);
            if (mural == null) return;

            if (!introduced.Contains(current))
            {
                introduced.Add(current);
                tapIndex = 0;
                queue.Clear();
                if (!greeted && lines.Hello != null) queue.Enqueue(lines.Hello);
                greeted = true;
                Enqueue(mural.intro);
                Enqueue(mural.about);
                Enqueue(mural.tryThis);
                nextLineAt = Time.time + 0.5f;
            }
            else
            {
                Say(tapIndex % 2 == 0 ? mural.about : mural.tryThis);
                tapIndex++;
            }
        }

        void PearlPressed()
        {
            if (IsSpeaking)
            {
                voice.Stop();
                nextLineAt = Time.time;
                return;
            }
            var mural = lines != null ? lines.For(current) : null;
            if (mural == null) return;
            Say(tapIndex % 2 == 0 ? mural.about : mural.tryThis);
            tapIndex++;
        }

        void HandleFound(MuralData data)
        {
            current = data;
            active = true;
            hasNews = data != null && !introduced.Contains(data);
        }

        void HandleLost(MuralData data)
        {
            if (data != current) return;
            Silence();
            Hide();
        }

        void HandleCompleted(MuralData data)
        {
            var mural = lines != null ? lines.For(data) : null;
            if (mural == null || mural.done == null) return;
            Show();
            Say(mural.done);
        }

        void HandleState(AppState state)
        {
            if (state == AppState.Start || state == AppState.Scanning)
            {
                active = false;
                Silence();
                Hide();
            }
        }

        void HandleReset()
        {
            introduced.Clear();
            Silence();
            Hide();
            hasNews = active;
        }

        void Say(AudioClip clip)
        {
            if (clip == null) return;
            queue.Clear();
            queue.Enqueue(clip);
            if (voice.isPlaying) voice.Stop();
            nextLineAt = Time.time;
        }

        void Enqueue(AudioClip clip)
        {
            if (clip != null) queue.Enqueue(clip);
        }

        void Silence()
        {
            queue.Clear();
            if (voice != null) voice.Stop();
            AudioManager.SetDucked(false);
        }

        void Update()
        {
            if (shown && !voice.isPlaying && queue.Count > 0 && Time.time >= nextLineAt)
            {
                voice.clip = queue.Dequeue();
                voice.Play();
            }

            bool talking = voice.isPlaying || queue.Count > 0;
            if (voice.isPlaying) nextLineAt = Time.time + gapBetweenLines;
            if (talking) quietSince = Time.time;
            if (shown) AudioManager.SetDucked(talking);
            if (shown && !talking && Time.time - quietSince > idleBeforeHide) Hide();

            Animate();
        }

        void Animate()
        {
            float t = Time.time;
            float rms = 0f;
            if (voice.isPlaying)
            {
                voice.GetOutputData(samples, 0);
                for (int i = 0; i < samples.Length; i++) rms += samples[i] * samples[i];
                rms = Mathf.Sqrt(rms / samples.Length);
            }
            float goal = Mathf.Clamp01(rms * mouthGain);
            level = Mathf.Lerp(level, goal, goal > level ? 0.6f : 0.2f);

            if (pearl != null)
            {
                AnimateRig(pearl, t, level);
                pearl.root.anchoredPosition = new Vector2(0f, Mathf.Sin(t * 1.6f) * 12f);
                float flap = (IsSpeaking ? 10f + level * 14f : 5f) * Mathf.Sin(t * (IsSpeaking ? 6f : 2f));
                if (pearl.finL != null) pearl.finL.localRotation = Quaternion.Euler(0f, 0f, -flap);
                if (pearl.finR != null) pearl.finR.localRotation = Quaternion.Euler(0f, 0f, flap);
            }

            if (icon != null)
            {
                AnimateRig(icon, t + 1.3f, 0f);
                float pulse = hasNews ? 1f + Mathf.Abs(Mathf.Sin(t * 3f)) * 0.08f : 1f;
                button.localScale = Vector3.one * pulse;
                if (buttonRing != null)
                {
                    var c = buttonRing.color;
                    c.a = hasNews ? 0.5f + Mathf.Abs(Mathf.Sin(t * 3f)) * 0.5f : 0f;
                    buttonRing.color = c;
                }
            }
        }

        static void AnimateRig(Rig rig, float t, float mouthLevel)
        {
            if (rig.mouth != null) rig.mouth.localScale = new Vector3(1f + mouthLevel * 0.25f, 0.35f + mouthLevel * 1.4f, 1f);
            if (t >= rig.nextBlinkAt)
            {
                rig.blinkUntil = t + 0.12f;
                rig.nextBlinkAt = t + Random.Range(2.5f, 5.5f);
            }
            bool blinking = t < rig.blinkUntil;
            rig.eyesOpen.SetActive(!blinking);
            rig.eyesClosed.SetActive(blinking);
        }

        void Show()
        {
            if (pearlRoot == null || shown) return;
            shown = true;
            quietSince = Time.time;
            pearlRoot.gameObject.SetActive(true);
            Pop(true);
        }

        void Hide()
        {
            if (pearlRoot == null || !shown) return;
            shown = false;
            AudioManager.SetDucked(false);
            Pop(false);
        }

        void Pop(bool open)
        {
            if (popping != null) StopCoroutine(popping);
            popping = StartCoroutine(PopRoutine(open));
        }

        IEnumerator PopRoutine(bool open)
        {
            Vector3 from = pearlRoot.localScale;
            Vector3 to = open ? Vector3.one : Vector3.zero;
            float duration = open ? 0.45f : 0.25f;
            yield return Tween.Run(duration, k => pearlRoot.localScale = Vector3.LerpUnclamped(from, to, k),
                open ? Tween.EaseOutBack : Tween.EaseInOutCubic);
            if (!open) pearlRoot.gameObject.SetActive(false);
            popping = null;
        }

        void Build()
        {
            var hud = FindAnyObjectByType<ARHud>(FindObjectsInactive.Include);
            Transform template = hud != null ? FindDeep(hud.transform, anchorButton) : null;
            if (template == null)
            {
                Debug.LogWarning("Narrator: couldn't find the HUD button to sit under.", this);
                return;
            }

            BuildButton(template);
            BuildPearl(hud.transform);
        }

        void BuildButton(Transform template)
        {
            var copy = Instantiate(template.gameObject, template.parent);
            copy.name = "NarratorButton";
            button = (RectTransform)copy.transform;
            button.anchoredPosition = ((RectTransform)template).anchoredPosition - new Vector2(0f, spacing);
            Transform border = null;
            for (int i = button.childCount - 1; i >= 0; i--)
            {
                var child = button.GetChild(i);
                if (child.name == "Border") border = child;
                else Destroy(child.gameObject);
            }

            var ui = copy.GetComponent<Button>();
            ui.onClick.RemoveAllListeners();
            ui.onClick.AddListener(ButtonPressed);
            var audio = FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (audio != null) ui.onClick.AddListener(audio.PlayButton);

            var ringGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
            var ringRect = ringGo.GetComponent<RectTransform>();
            ringRect.SetParent(button.parent, false);
            ringRect.SetSiblingIndex(button.GetSiblingIndex());
            ringRect.anchorMin = button.anchorMin;
            ringRect.anchorMax = button.anchorMax;
            ringRect.pivot = button.pivot;
            ringRect.sizeDelta = button.sizeDelta + new Vector2(16f, 16f);
            ringRect.anchoredPosition = button.anchoredPosition + new Vector2(8f, 8f);
            buttonRing = ringGo.GetComponent<Image>();
            var circle = copy.GetComponent<Image>();
            buttonRing.sprite = circle != null ? circle.sprite : null;
            buttonRing.color = new Color(0.82f, 0.05f, 0.18f, 0f);
            buttonRing.raycastTarget = false;

            var maskHolder = new GameObject("Mask", typeof(RectTransform), typeof(Image), typeof(Mask));
            var maskRect = maskHolder.GetComponent<RectTransform>();
            maskRect.SetParent(button, false);
            Stretch(maskRect);
            var maskImage = maskHolder.GetComponent<Image>();
            maskImage.sprite = buttonRing.sprite;
            maskImage.raycastTarget = false;
            maskHolder.GetComponent<Mask>().showMaskGraphic = false;

            float h = button.sizeDelta.y * 2.1f;
            var holder = NewRect("Face", maskRect, new Vector2(h * CanvasWidth / CanvasHeight, h));
            holder.anchoredPosition = new Vector2(0f, -(390f - 355f) / CanvasHeight * h);
            icon = BuildRig(holder, false);
            if (border != null) border.SetAsLastSibling();
        }

        void BuildPearl(Transform hud)
        {
            pearlRoot = NewRect("Pearl", hud, new Vector2(pearlHeight * CanvasWidth / CanvasHeight, pearlHeight));
            pearlRoot.anchorMin = pearlRoot.anchorMax = new Vector2(0.5f, 0f);
            pearlRoot.pivot = new Vector2(0.5f, 0f);
            pearlRoot.anchoredPosition = new Vector2(0f, bottomMargin);

            var bob = NewRect("Bob", pearlRoot, pearlRoot.sizeDelta);
            pearl = BuildRig(bob, true);

            var hit = bob.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            hit.raycastTarget = true;
            bob.gameObject.AddComponent<Button>().onClick.AddListener(PearlPressed);

            pearlRoot.localScale = Vector3.zero;
            pearlRoot.gameObject.SetActive(false);
        }

        Rig BuildRig(RectTransform holder, bool full)
        {
            var rig = new Rig { root = holder };
            if (full)
            {
                Part(glow, holder);
                rig.finL = Pivot("Fin L", holder, 170f, 425f);
                Part(finLeft, rig.finL, holder);
                rig.finR = Pivot("Fin R", holder, 430f, 425f);
                Part(finRight, rig.finR, holder);
            }
            Part(body, holder);
            Part(sockets, holder);

            rig.eyesOpen = Group("Eyes Open", holder);
            Part(eyeLeft, rig.eyesOpen.transform);
            Part(eyeRight, rig.eyesOpen.transform);
            Part(lashes, rig.eyesOpen.transform);
            rig.eyesClosed = Group("Eyes Closed", holder);
            Part(eyesBlink, rig.eyesClosed.transform);
            rig.eyesClosed.SetActive(false);

            rig.mouth = Part(mouth, holder);
            if (rig.mouth != null) rig.mouth.pivot = new Vector2(0.5f, 1f - 388f / CanvasHeight);
            if (full) Part(shine, holder);
            return rig;
        }

        RectTransform Pivot(string pivotName, RectTransform holder, float x, float y)
        {
            var pivot = NewRect(pivotName, holder, Vector2.zero);
            Vector2 size = holder.sizeDelta;
            pivot.anchoredPosition = new Vector2((x - CanvasWidth * 0.5f) / CanvasWidth * size.x, (CanvasHeight * 0.5f - y) / CanvasHeight * size.y);
            return pivot;
        }

        GameObject Group(string groupName, RectTransform holder)
        {
            var rect = NewRect(groupName, holder, Vector2.zero);
            Stretch(rect);
            return rect.gameObject;
        }

        RectTransform Part(Sprite sprite, Transform parent, RectTransform sizeFrom = null)
        {
            if (sprite == null) return null;
            var go = new GameObject(sprite.name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            if (sizeFrom != null)
            {
                rect.sizeDelta = sizeFrom.sizeDelta;
                rect.anchoredPosition = -((RectTransform)parent).anchoredPosition;
            }
            else
            {
                Stretch(rect);
            }
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return rect;
        }

        static RectTransform NewRect(string rectName, Transform parent, Vector2 size)
        {
            var rect = new GameObject(rectName, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            return rect;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static Transform FindDeep(Transform root, string childName)
        {
            if (root.name == childName) return root;
            foreach (Transform child in root)
            {
                var found = FindDeep(child, childName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
