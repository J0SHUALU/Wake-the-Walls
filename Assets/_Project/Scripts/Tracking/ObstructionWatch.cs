using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    // Compares what the camera sees on the wall with the painting we already have. Anything that is not
    // part of the mural, such as a hand, a person or a bag, makes the view differ, and the mural pauses
    // as if it slipped out of view. Painted figures match the painting, so they never count.
    public class ObstructionWatch : MonoBehaviour
    {
        public static bool Blocked { get; private set; }

        [SerializeField] ARCameraBackground background;
        [Tooltip("Width of the small camera picture used for the check.")]
        [SerializeField, Range(24, 128)] int sampleWidth = 48;
        [SerializeField, Range(2f, 20f)] float checksPerSecond = 7f;
        [Tooltip("How far a spot can differ from the painting, in standard deviations, before it counts as covered.")]
        [SerializeField, Range(0.5f, 3f)] float spotTolerance = 1.3f;
        [Tooltip("How much more of the mural has to look different than when the view was clearest, before it pauses.")]
        [SerializeField, Range(0.02f, 0.6f)] float blockShare = 0.07f;
        [SerializeField, Range(0.01f, 0.5f)] float clearShare = 0.035f;
        [SerializeField, Min(0f)] float blockAfter = 0.2f;
        [SerializeField, Min(0f)] float clearAfter = 0.4f;
        [Tooltip("The clearest view seen so far is the reference. It only creeps up this much per second, for changes in light.")]
        [SerializeField, Range(0f, 0.05f)] float baselineDrift = 0.004f;
        [Tooltip("Writes what the check sees to the log every couple of seconds.")]
        [SerializeField] bool logChecks = true;
        float nextLog;

        static readonly Dictionary<MuralData, float[]> pictures = new Dictionary<MuralData, float[]>();
        static readonly Dictionary<MuralData, Vector2Int> pictureSizes = new Dictionary<MuralData, Vector2Int>();

        Camera view;
        RenderTexture shot;
        MuralExperience focused;
        bool waiting, flipped, flipKnown;
        [Tooltip("Typical share of a clear view that looks different from the painting, from light and small tracking offsets.")]
        [SerializeField, Range(0f, 0.5f)] float typicalClear = 0.12f;

        float nextCheck, baseline = -1f, lastCheck, overSince = -1f, underSince = -1f;
        readonly List<Vector2> pairs = new List<Vector2>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Watch()
        {
            Blocked = false;
            SceneManager.sceneLoaded -= Install;
            SceneManager.sceneLoaded += Install;
        }

        static void Install(Scene scene, LoadSceneMode mode)
        {
            if (FindAnyObjectByType<ObstructionWatch>() != null) return;
            var bg = FindAnyObjectByType<ARCameraBackground>();
            if (bg == null) return;
            var watch = bg.gameObject.AddComponent<ObstructionWatch>();
            watch.background = bg;
            // Added from code, so the inspector toggle never applies: only log in development builds.
            watch.logChecks = Debug.isDebugBuild;
        }

        void OnEnable()
        {
            GameEvents.MuralFocused += OnFocused;
            GameEvents.MuralFound += OnFound;
            GameEvents.ResetRequested += OnReset;
        }

        void OnDisable()
        {
            GameEvents.MuralFocused -= OnFocused;
            GameEvents.MuralFound -= OnFound;
            GameEvents.ResetRequested -= OnReset;
            Blocked = false;
        }

        void OnDestroy()
        {
            if (shot != null) shot.Release();
        }

        void OnFocused(MuralData mural)
        {
            focused = null;
            foreach (MuralExperience experience in FindObjectsByType<MuralExperience>(FindObjectsInactive.Exclude))
                if (experience.Data == mural) focused = experience;
            baseline = -1f;
        }

        void OnFound(MuralData mural)
        {
            if (focused == null || focused.Data != mural) OnFocused(mural);
            if (!Blocked) baseline = -1f;
        }

        void OnReset()
        {
            Blocked = false;
            baseline = -1f;
        }

        void Update()
        {
            if (waiting || Time.time < nextCheck || focused == null) return;
            if (background == null || background.material == null)
            {
                if (logChecks && Time.time >= nextLog) { nextLog = Time.time + 2f; Debug.Log("[Obstruction] no camera background material yet"); }
                return;
            }
            if (focused.Data == null || focused.Data.CheckImage == null) return;
            if (view == null) view = background.GetComponent<Camera>();
            if (view == null) return;
            nextCheck = Time.time + 1f / checksPerSecond;

            int h = Mathf.Max(8, Mathf.RoundToInt(sampleWidth * Screen.height / (float)Mathf.Max(1, Screen.width)));
            if (shot == null || shot.width != sampleWidth || shot.height != h)
            {
                if (shot != null) shot.Release();
                shot = new RenderTexture(sampleWidth, h, 0, RenderTextureFormat.ARGB32);
            }
            Graphics.Blit(null, shot, background.material);
            waiting = true;
            AsyncGPUReadback.Request(shot, 0, TextureFormat.RGBA32, OnShot);
        }

        void OnShot(AsyncGPUReadbackRequest request)
        {
            waiting = false;
            if (request.hasError || focused == null || view == null) return;
            NativeArray<Color32> pixels = request.GetData<Color32>();
            int w = request.width, h = request.height;

            float straight = Mismatch(pixels, w, h, false);
            if (!flipKnown)
            {
                // The camera picture can come back upside down on some phones; keep whichever way lines up with the painting.
                float upside = Mismatch(pixels, w, h, true);
                if (straight >= 0f && upside >= 0f) { flipped = upside < straight; flipKnown = Mathf.Abs(upside - straight) > 0.1f; }
            }
            float share = flipped ? Mismatch(pixels, w, h, true) : straight;
            if (share >= 0f) Judge(share);
            else if (logChecks && Time.time >= nextLog) { nextLog = Time.time + 2f; Debug.Log("[Obstruction] too little of the mural on screen to check"); }
        }

        // Share of the visible mural that does not look like the painting, or -1 when too little of it is on screen.
        float Mismatch(NativeArray<Color32> pixels, int w, int h, bool flip)
        {
            MuralData data = focused.Data;
            float[] picture = Picture(data, out Vector2Int size);
            Vector2 mural = data.PhysicalSizeMeters;
            Transform wall = focused.transform;
            var plane = new Plane(wall.forward, wall.position);
            pairs.Clear();
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var vp = new Vector3((x + 0.5f) / w, (flip ? h - 1 - y + 0.5f : y + 0.5f) / h, 0f);
                Ray ray = view.ViewportPointToRay(vp);
                if (!plane.Raycast(ray, out float d)) continue;
                Vector3 local = wall.InverseTransformPoint(ray.GetPoint(d));
                float u = local.x / mural.x + 0.5f, v = local.y / mural.y + 0.5f;
                if (u < 0.03f || u > 0.97f || v < 0.03f || v > 0.97f) continue;
                Color32 c = pixels[y * w + x];
                float seen = (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) / 255f;
                pairs.Add(new Vector2(seen, Sample(picture, size, u, v)));
            }
            if (pairs.Count < 60) return -1f;

            float ms = 0f, me = 0f;
            foreach (Vector2 p in pairs) { ms += p.x; me += p.y; }
            ms /= pairs.Count; me /= pairs.Count;
            float vs = 0f, ve = 0f;
            foreach (Vector2 p in pairs) { vs += (p.x - ms) * (p.x - ms); ve += (p.y - me) * (p.y - me); }
            float ss = Mathf.Sqrt(vs / pairs.Count) + 1e-3f, se = Mathf.Sqrt(ve / pairs.Count) + 1e-3f;
            int off = 0;
            foreach (Vector2 p in pairs)
                if (Mathf.Abs((p.x - ms) / ss - (p.y - me) / se) > spotTolerance) off++;
            return off / (float)pairs.Count;
        }

        void Judge(float share)
        {
            float now = Time.time;
            float dt = lastCheck > 0f ? now - lastCheck : 0f;
            lastCheck = now;
            // The clearest view of the mural so far is what "nothing in the way" looks like. Someone already standing
            // in front when the mural is found still counts, because a clearer view sets the reference as soon as they move,
            // and the starting reference is the typical mismatch of a clear view rather than whatever was seen first.
            if (baseline < 0f) baseline = Mathf.Min(share, typicalClear);
            if (share < baseline) baseline = share;
            else if (!Blocked) baseline = Mathf.Min(share, baseline + baselineDrift * dt);
            float extra = share - baseline;
            if (logChecks && now >= nextLog)
            {
                nextLog = now + 2f;
                Debug.Log($"[Obstruction] share {share:0.00} clear {baseline:0.00} blocked {Blocked} flipped {flipped}");
            }
            if (!Blocked)
            {
                overSince = extra >= blockShare ? (overSince < 0f ? now : overSince) : -1f;
                if (overSince >= 0f && now - overSince >= blockAfter) { Blocked = true; underSince = -1f; }
            }
            else
            {
                underSince = extra <= clearShare ? (underSince < 0f ? now : underSince) : -1f;
                if (underSince >= 0f && now - underSince >= clearAfter) { Blocked = false; overSince = -1f; }
            }
        }

        static float[] Picture(MuralData data, out Vector2Int size)
        {
            if (pictures.TryGetValue(data, out float[] cached)) { size = pictureSizes[data]; return cached; }
            Texture2D tex = data.CheckImage;
            Color32[] px = tex.GetPixels32();
            var values = new float[px.Length];
            for (int i = 0; i < px.Length; i++) values[i] = (0.299f * px[i].r + 0.587f * px[i].g + 0.114f * px[i].b) / 255f;
            size = new Vector2Int(tex.width, tex.height);
            pictures[data] = values;
            pictureSizes[data] = size;
            return values;
        }

        static float Sample(float[] picture, Vector2Int size, float u, float v)
        {
            float x = u * (size.x - 1), y = v * (size.y - 1);
            int x0 = (int)x, y0 = (int)y, x1 = Mathf.Min(x0 + 1, size.x - 1), y1 = Mathf.Min(y0 + 1, size.y - 1);
            float fx = x - x0, fy = y - y0;
            float a = Mathf.Lerp(picture[y0 * size.x + x0], picture[y0 * size.x + x1], fx);
            float b = Mathf.Lerp(picture[y1 * size.x + x0], picture[y1 * size.x + x1], fx);
            return Mathf.Lerp(a, b, fy);
        }
    }
}
