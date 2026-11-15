using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    // Watches the phone's person outline. When a hand or a person covers enough of the view, the mural
    // pauses as if it slipped out of view, instead of being cut around them.
    public class PeopleOcclusion : MonoBehaviour
    {
        // True while something is in the way of the camera.
        public static bool Blocked { get; private set; }

        static readonly int Stencil = Shader.PropertyToID("_WtwHumanStencil");
        static readonly int ArkitStencil = Shader.PropertyToID("_HumanStencil");
        static readonly int StencilTexel = Shader.PropertyToID("_WtwStencilTexel");
        static readonly int Display = Shader.PropertyToID("_WtwDisplayTransform");
        static readonly int Cut = Shader.PropertyToID("_WtwPeopleCut");
        static readonly int Flip = Shader.PropertyToID("_WtwFlip");
        static readonly int Rim = Shader.PropertyToID("_WtwRim");
        static readonly int ArkitDepth = Shader.PropertyToID("_HumanDepth");
        static readonly int Depth = Shader.PropertyToID("_WtwHumanDepth");
        static readonly int WallPlane = Shader.PropertyToID("_WtwWallPlane");
        static readonly int HasWall = Shader.PropertyToID("_WtwHasWall");
        static readonly int WallGap = Shader.PropertyToID("_WtwWallGap");

        [SerializeField] AROcclusionManager occlusion;
        [SerializeField] ARCameraManager cameraManager;
        [SerializeField] HumanSegmentationStencilMode quality = HumanSegmentationStencilMode.Disabled;
        [Tooltip("Turn on if the cut-out shows upside down on a device.")]
        [SerializeField] bool flipVertical;
        [Tooltip("Seconds for the gold edge to fade in and out with the mural.")]
        [SerializeField, Range(0.1f, 2f)] float rimFade = 0.6f;
        [Tooltip("A person only cuts through the mural when they stand at least this many metres in front of the wall, so painted figures on the wall are left alone.")]
        [SerializeField, Range(0f, 1f)] float minDistanceFromWall = 0.25f;

        [Header("Blocking")]
        [Tooltip("How much more of the view a hand or person has to cover, on top of what was already there, before the mural pauses.")]
        [SerializeField, Range(0.01f, 0.5f)] float blockShare = 0.07f;
        [Tooltip("The extra cover has to drop below this before the mural carries on.")]
        [SerializeField, Range(0.005f, 0.3f)] float clearShare = 0.03f;
        [Tooltip("Seconds the steady part of the outline takes to follow the view. Painted figures stay steady as you move, so they never pause the mural; a hand or person coming in is sudden.")]
        [SerializeField, Range(0.3f, 6f)] float settleSeconds = 1.5f;
        [SerializeField, Min(0f)] float blockAfter = 0.2f;
        [SerializeField, Min(0f)] float clearAfter = 0.4f;
        [Tooltip("How often the outline is checked, per second.")]
        [SerializeField, Range(2f, 30f)] float checksPerSecond = 8f;

        float nextCheck, overSince = -1f, underSince = -1f, baseline = -1f, lastCheck;

        bool haveVideo;
        Texture outline;
        Texture depth;
        Transform wall;
        float outlineSeenAt = -10f;
        float rim, rimTarget;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Watch()
        {
            Shader.SetGlobalFloat(Cut, 0f);
            Shader.SetGlobalFloat(Rim, 0f);
            Shader.SetGlobalMatrix(Display, Matrix4x4.identity);
            SceneManager.sceneLoaded -= Install;
            SceneManager.sceneLoaded += Install;
        }

        static void Install(Scene scene, LoadSceneMode mode)
        {
            if (FindFirstObjectByType<PeopleOcclusion>() != null) return;
            var manager = FindFirstObjectByType<AROcclusionManager>();
            var cam = FindFirstObjectByType<ARCameraManager>();
            if (manager == null || cam == null) return;

            var people = cam.gameObject.AddComponent<PeopleOcclusion>();
            people.Connect(manager, cam);
        }

        void Connect(AROcclusionManager manager, ARCameraManager cam)
        {
            occlusion = manager;
            cameraManager = cam;
            Subscribe();
        }

        void OnEnable() => Subscribe();

        void OnDisable()
        {
            if (cameraManager != null) cameraManager.frameReceived -= OnFrame;
            if (occlusion != null) occlusion.frameReceived -= OnOcclusionFrame;
            GameEvents.MuralFound -= OnFound;
            GameEvents.MuralLost -= OnLost;
            GameEvents.MuralFocused -= OnFocused;
            Shader.SetGlobalFloat(Cut, 0f);
            Shader.SetGlobalFloat(Rim, 0f);
            Blocked = false;
        }

        void Subscribe()
        {
            if (occlusion == null || cameraManager == null) return;
            occlusion.requestedHumanDepthMode = HumanSegmentationDepthMode.Disabled;
            occlusion.requestedEnvironmentDepthMode = EnvironmentDepthMode.Disabled;
            occlusion.requestedHumanStencilMode = quality;

            cameraManager.frameReceived -= OnFrame;
            cameraManager.frameReceived += OnFrame;
            occlusion.frameReceived -= OnOcclusionFrame;
            occlusion.frameReceived += OnOcclusionFrame;
            GameEvents.MuralFound -= OnFound;
            GameEvents.MuralFound += OnFound;
            GameEvents.MuralLost -= OnLost;
            GameEvents.MuralLost += OnLost;
            GameEvents.MuralFocused -= OnFocused;
            GameEvents.MuralFocused += OnFocused;
        }

        void UpdateBlocked()
        {
            float share = 0f;
            if (occlusion != null && occlusion.TryAcquireHumanStencilCpuImage(out XRCpuImage image))
            {
                using (image)
                {
                    XRCpuImage.Plane plane = image.GetPlane(0);
                    var data = plane.data;
                    int w = image.width, h = image.height, hits = 0, total = 0;
                    int step = Mathf.Max(1, Mathf.Min(w, h) / 48);
                    for (int y = 0; y < h; y += step)
                    for (int x = 0; x < w; x += step)
                    {
                        total++;
                        if (data[y * plane.rowStride + x * plane.pixelStride] > 127) hits++;
                    }
                    share = total > 0 ? hits / (float)total : 0f;
                }
            }

            // Whatever the phone sees as a person all the time, such as a painted face on the mural, becomes the
            // baseline. Only a quick jump above it counts as something in the way. The baseline holds still while blocked.
            float now = Time.time;
            float dt = lastCheck > 0f ? now - lastCheck : 0f;
            lastCheck = now;
            if (baseline < 0f) baseline = share;
            if (!Blocked)
            {
                if (share < baseline) baseline = share;
                else baseline = Mathf.MoveTowards(baseline, share, (share - baseline) * Mathf.Clamp01(dt / settleSeconds));
            }
            float extra = share - baseline;
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

        void OnFocused(MuralData mural)
        {
            wall = null;
            foreach (MuralExperience experience in FindObjectsByType<MuralExperience>(FindObjectsSortMode.None))
            {
                if (experience.Data == mural) wall = experience.transform;
            }
        }

        void OnFound(MuralData mural)
        {
            rimTarget = 1f;
            if (!Blocked) baseline = -1f;
        }

        void OnLost(MuralData mural) => rimTarget = 0f;

        void OnFrame(ARCameraFrameEventArgs args)
        {
            if (args.displayMatrix.HasValue) Shader.SetGlobalMatrix(Display, args.displayMatrix.Value);
            if (args.textures == null || args.propertyNameIds == null) return;
            for (int i = 0; i < args.textures.Count; i++)
                Shader.SetGlobalTexture(args.propertyNameIds[i], args.textures[i]);
            haveVideo = args.textures.Count >= 2;
        }

        // Reading the outline from the frame event instead of AROcclusionManager.humanStencilTexture,
        // which logged every frame on device and the app ran out of memory.
        void OnOcclusionFrame(AROcclusionFrameEventArgs args)
        {
            foreach (ARExternalTexture texture in args.externalTextures)
            {
                if (texture.texture == null) continue;
                if (texture.propertyId == ArkitDepth) depth = texture.texture;
                if (texture.propertyId != ArkitStencil) continue;
                outline = texture.texture;
                outlineSeenAt = Time.time;
            }
        }

        void Update()
        {
            bool on = outline != null && haveVideo && Time.time - outlineSeenAt < 0.5f;
            if (on)
            {
                Shader.SetGlobalTexture(Stencil, outline);
                Shader.SetGlobalVector(StencilTexel, new Vector4(1f / outline.width, 1f / outline.height, 0f, 0f));
            }
            Shader.SetGlobalFloat(Cut, 0f);
            Shader.SetGlobalFloat(Flip, flipVertical ? 1f : 0f);

            bool hasWall = on && depth != null && wall != null;
            if (hasWall)
            {
                Vector3 normal = wall.forward;
                Shader.SetGlobalTexture(Depth, depth);
                Shader.SetGlobalVector(WallPlane, new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, wall.position)));
                Shader.SetGlobalFloat(WallGap, minDistanceFromWall);
            }
            Shader.SetGlobalFloat(HasWall, hasWall ? 1f : 0f);

            Shader.SetGlobalFloat(Rim, 0f);

        }
    }
}
