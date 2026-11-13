using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    // Keeps people in front of the murals. It shares the phone's person outline and camera image
    // with the PeopleCut full screen pass, which paints the camera image back over anyone in view.
    // Depth is switched off on purpose: the layers sit millimetres off the wall, far closer than
    // the phone's depth guess is accurate, and that made people flicker in and out.
    public class PeopleOcclusion : MonoBehaviour
    {
        static readonly int Stencil = Shader.PropertyToID("_WtwHumanStencil");
        static readonly int ArkitStencil = Shader.PropertyToID("_HumanStencil");
        static readonly int StencilTexel = Shader.PropertyToID("_WtwStencilTexel");
        static readonly int Display = Shader.PropertyToID("_WtwDisplayTransform");
        static readonly int Cut = Shader.PropertyToID("_WtwPeopleCut");
        static readonly int Flip = Shader.PropertyToID("_WtwFlip");
        static readonly int Rim = Shader.PropertyToID("_WtwRim");

        [SerializeField] AROcclusionManager occlusion;
        [SerializeField] ARCameraManager cameraManager;
        [SerializeField] HumanSegmentationStencilMode quality = HumanSegmentationStencilMode.Medium;
        [Tooltip("Turn on if the cut-out shows upside down on a device.")]
        [SerializeField] bool flipVertical;
        [Tooltip("Seconds for the gold edge to fade in and out with the mural.")]
        [SerializeField, Range(0.1f, 2f)] float rimFade = 0.6f;

        bool haveVideo;
        Texture outline;
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
            Shader.SetGlobalFloat(Cut, 0f);
            Shader.SetGlobalFloat(Rim, 0f);
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
        }

        void OnFound(MuralData mural) => rimTarget = 1f;

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
                if (texture.propertyId != ArkitStencil || texture.texture == null) continue;
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
            Shader.SetGlobalFloat(Cut, on ? 1f : 0f);
            Shader.SetGlobalFloat(Flip, flipVertical ? 1f : 0f);

            rim = Mathf.MoveTowards(rim, rimTarget, Time.deltaTime / rimFade);
            Shader.SetGlobalFloat(Rim, rim);
        }
    }
}
