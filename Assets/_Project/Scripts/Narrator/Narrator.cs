using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Audio;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Narrator
{
    [RequireComponent(typeof(AudioSource))]
    public class Narrator : MonoBehaviour
    {
        const float CanvasWidth = 600f;
        const float CanvasHeight = 780f;
        const float CanvasCentreY = 390f;

        [SerializeField] NarratorLines lines;

        [Header("Parts (600 x 780 canvas, centre pivot, 780 pixels per unit)")]
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

        [Header("Placement")]
        [Tooltip("Pearl's height as a share of the mural height.")]
        [SerializeField] float heightShare = 0.18f;
        [SerializeField] float minHeight = 0.35f;
        [SerializeField] float maxHeight = 0.9f;
        [Tooltip("How far in front of the wall she floats, in metres.")]
        [SerializeField] float forward = 0.25f;
        [SerializeField] float followSpeed = 8f;

        [Header("Voice")]
        [SerializeField] float gapBetweenLines = 0.35f;
        [SerializeField] float mouthGain = 9f;

        AudioSource voice;
        Transform visual;
        Transform finL, finR, mouthPivot;
        GameObject eyesOpen, eyesClosed;

        MuralExperience target;
        MuralData current;
        readonly Queue<AudioClip> queue = new Queue<AudioClip>();
        readonly HashSet<MuralData> introduced = new HashSet<MuralData>();
        readonly float[] samples = new float[256];
        bool greeted;
        bool shown;
        int tapIndex;
        float level;
        float nextLineAt;
        float nextBlinkAt;
        float blinkUntil;
        float height = 0.5f;
        Coroutine popping;

        void Awake()
        {
            voice = GetComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0f;
            BuildVisual();
            visual.gameObject.SetActive(false);
        }

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

        public bool IsSpeaking => voice != null && voice.isPlaying;

        public void Tap()
        {
            if (!shown || lines == null) return;
            if (IsSpeaking)
            {
                voice.Stop();
                nextLineAt = Time.time;
                return;
            }

            var mural = lines.For(current);
            if (mural == null) return;
            AudioClip next = tapIndex % 2 == 0 ? mural.about : mural.tryThis;
            tapIndex++;
            Say(next);
        }

        void HandleFound(MuralData data)
        {
            current = data;
            target = FindExperience(data);
            if (target == null) return;

            if (data != null) height = Mathf.Clamp(data.PhysicalSizeMeters.y * heightShare, minHeight, maxHeight);
            transform.position = TargetPosition();
            Show();

            if (lines == null || introduced.Contains(data)) return;
            introduced.Add(data);
            tapIndex = 0;

            queue.Clear();
            if (!greeted && lines.Hello != null) queue.Enqueue(lines.Hello);
            greeted = true;

            var mural = lines.For(data);
            if (mural == null) return;
            Enqueue(mural.intro);
            Enqueue(mural.about);
            Enqueue(mural.tryThis);
            nextLineAt = Time.time + 0.6f;
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
            Silence();
            Say(mural.done);
        }

        void HandleState(AppState state)
        {
            if (state == AppState.Start || state == AppState.Scanning)
            {
                Silence();
                Hide();
            }
        }

        void HandleReset()
        {
            introduced.Clear();
            Silence();
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
        }

        void Update()
        {
            if (!shown) return;

            if (!voice.isPlaying && queue.Count > 0 && Time.time >= nextLineAt)
            {
                voice.clip = queue.Dequeue();
                voice.Play();
            }
            if (voice.isPlaying) nextLineAt = Time.time + gapBetweenLines;

            AudioManager.SetDucked(voice.isPlaying || queue.Count > 0);
            Animate();
        }

        void LateUpdate()
        {
            if (!shown) return;
            if (target != null)
            {
                transform.position = Vector3.Lerp(transform.position, TargetPosition(), 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            }

            var cam = Camera.main;
            if (cam == null) return;
            Vector3 away = transform.position - cam.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(away, Vector3.up);
        }

        Vector3 TargetPosition()
        {
            Vector2 size = current != null ? current.PhysicalSizeMeters : Vector2.one;
            var local = new Vector3(-size.x * 0.5f + height * 0.4f, -size.y * 0.5f + height * 0.6f, -forward);
            return target.transform.TransformPoint(local);
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

            visual.localPosition = new Vector3(0f, Mathf.Sin(t * 1.6f) * 0.03f, 0f);
            mouthPivot.localScale = new Vector3(1f + level * 0.25f, 0.35f + level * 1.4f, 1f);

            float flap = (IsSpeaking ? 10f + level * 14f : 5f) * Mathf.Sin(t * (IsSpeaking ? 6f : 2f));
            finL.localRotation = Quaternion.Euler(0f, 0f, -flap);
            finR.localRotation = Quaternion.Euler(0f, 0f, flap);

            if (t >= nextBlinkAt)
            {
                blinkUntil = t + 0.12f;
                nextBlinkAt = t + Random.Range(2.5f, 5.5f);
            }
            bool blinking = t < blinkUntil;
            eyesOpen.SetActive(!blinking);
            eyesClosed.SetActive(blinking);
        }

        void Show()
        {
            if (shown) return;
            shown = true;
            visual.gameObject.SetActive(true);
            Pop(Vector3.one * height);
        }

        void Hide()
        {
            if (!shown) return;
            shown = false;
            AudioManager.SetDucked(false);
            Pop(Vector3.zero);
        }

        void Pop(Vector3 to)
        {
            if (popping != null) StopCoroutine(popping);
            popping = StartCoroutine(PopRoutine(to));
        }

        IEnumerator PopRoutine(Vector3 to)
        {
            Transform root = visual.parent;
            bool hiding = to == Vector3.zero;
            yield return Tween.Scale(root, to, hiding ? 0.25f : 0.5f, hiding ? Tween.EaseInOutCubic : Tween.EaseOutBack);
            if (hiding) visual.gameObject.SetActive(false);
            popping = null;
        }

        MuralExperience FindExperience(MuralData data)
        {
            foreach (var experience in FindObjectsByType<MuralExperience>(FindObjectsSortMode.None))
            {
                if (experience.Data == data) return experience;
            }
            return null;
        }

        void BuildVisual()
        {
            var scaler = new GameObject("Scaler").transform;
            scaler.SetParent(transform, false);
            scaler.localScale = Vector3.zero;

            visual = new GameObject("Pearl").transform;
            visual.SetParent(scaler, false);

            Part(glow, visual, 0);
            finL = Pivot("Fin L", 170f, 425f);
            Part(finLeft, finL, 1);
            finR = Pivot("Fin R", 430f, 425f);
            Part(finRight, finR, 1);
            Part(body, visual, 2);
            Part(sockets, visual, 3);

            eyesOpen = new GameObject("Eyes Open");
            eyesOpen.transform.SetParent(visual, false);
            Part(eyeLeft, eyesOpen.transform, 4);
            Part(eyeRight, eyesOpen.transform, 4);
            Part(lashes, eyesOpen.transform, 5);

            eyesClosed = new GameObject("Eyes Closed");
            eyesClosed.transform.SetParent(visual, false);
            Part(eyesBlink, eyesClosed.transform, 4);
            eyesClosed.SetActive(false);

            mouthPivot = Pivot("Mouth", 300f, 388f);
            Part(mouth, mouthPivot, 6);
            Part(shine, visual, 7);

            var box = gameObject.GetComponent<BoxCollider>();
            if (box == null) box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = Vector3.zero;
            box.size = new Vector3(0.6f, 0.75f, 0.2f);
            box.enabled = true;
            StartCoroutine(SyncCollider(box, scaler));
        }

        IEnumerator SyncCollider(BoxCollider box, Transform scaler)
        {
            while (true)
            {
                float s = scaler.localScale.y;
                box.size = new Vector3(0.6f * s, 0.75f * s, 0.2f * Mathf.Max(s, 0.01f));
                box.center = new Vector3(0f, 0.03f * s, 0f);
                box.enabled = s > 0.05f;
                yield return null;
            }
        }

        Transform Pivot(string pivotName, float x, float y)
        {
            var pivot = new GameObject(pivotName).transform;
            pivot.SetParent(visual, false);
            pivot.localPosition = CanvasToLocal(x, y);
            return pivot;
        }

        void Part(Sprite sprite, Transform parent, int order)
        {
            if (sprite == null) return;
            var go = new GameObject(sprite.name);
            go.transform.SetParent(parent, false);
            Vector3 offset = parent == visual ? Vector3.zero : -parent.localPosition;
            go.transform.localPosition = offset + new Vector3(0f, 0f, -0.002f * order);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 100 + order;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        static Vector3 CanvasToLocal(float x, float y) =>
            new Vector3((x - CanvasWidth * 0.5f) / CanvasHeight, (CanvasCentreY - y) / CanvasHeight, 0f);
    }
}
