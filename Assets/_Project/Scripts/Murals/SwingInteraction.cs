using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using WakeTheWalls.Interaction;
using WakeTheWalls.Rig;
using WakeTheWalls.VFX;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// A tap area over part of a mural that swings one or more layers hard, lets them settle back
    /// into their idle sway, and can throw out a paint burst and play a sound. Used for gusts
    /// through trees and swinging jewellery.
    /// </summary>
    /// <remarks>
    /// The tap area is a rectangle in mural space (0 to 1, with 0,0 at the bottom left), so it
    /// fits the mural at whatever real size the rig is built. It only reacts while the mural is visible.
    /// </remarks>
    [RequireComponent(typeof(BoxCollider), typeof(Interactable))]
    public class SwingInteraction : MonoBehaviour
    {
        [SerializeField] MuralExperience experience;
        [SerializeField] LayeredMuralRig rig;
        [SerializeField] LayerMotionSet motionSet;

        [Header("Tap area (0 to 1 across the mural)")]
        [SerializeField] Rect area = new Rect(0.4f, 0.4f, 0.2f, 0.2f);

        [Header("Swing")]
        [Tooltip("Elements that swing when this area is tapped.")]
        [SerializeField] string[] elements = new string[0];
        [SerializeField] float swingDegrees = 8f;
        [Tooltip("Swings per second while it settles.")]
        [SerializeField] float swingFrequency = 0.8f;
        [SerializeField, Range(0.6f, 4f)] float settleSeconds = 2.5f;

        [Header("Extras")]
        [SerializeField] PaintBurst burstPrefab;
        [Tooltip("Element the burst comes from. Leave empty to burst at the centre of the tap area.")]
        [SerializeField] string burstFrom;
        [SerializeField] Vector3 burstOffset = new Vector3(0f, 0.25f, -0.03f);
        [SerializeField] AudioClip sound;
        [Tooltip("Raised with this id each time the area is used, for example to report the interaction.")]
        [SerializeField] string interactionId = "swing";
        [SerializeField] UnityEvent<string> used = new UnityEvent<string>();

        Coroutine swing;
        AudioSource audioSource;
        float[] baseDegrees;

        void Start()
        {
            GetComponent<Interactable>().Tapped.AddListener(OnTapped);
            PlaceCollider();
        }

        /// <summary>Fits the box collider to the tap area at the rig's current size.</summary>
        public void PlaceCollider()
        {
            Vector2 size = rig.MuralSize;
            var box = GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3((area.center.x - 0.5f) * size.x, (area.center.y - 0.5f) * size.y, -0.03f);
            box.size = new Vector3(area.width * size.x, area.height * size.y, 0.06f);
        }

        void OnTapped()
        {
            if (!rig.IsBuilt || rig.Background.Alpha < 0.5f) return;
            if (swing != null) StopCoroutine(swing);
            swing = StartCoroutine(Swing());
            SpawnBurst();
            PlaySound();
            used.Invoke(interactionId);
        }

        IEnumerator Swing()
        {
            CaptureBase();
            float time = 0f;
            while (time < settleSeconds)
            {
                time += Time.deltaTime;
                // Ease in over the first fifth of a swing, then die away smoothly.
                float rise = Mathf.Clamp01(time * swingFrequency * 5f);
                float decay = Mathf.Exp(-3f * time / settleSeconds);
                SetExtra(swingDegrees * rise * decay * Mathf.Sin(2f * Mathf.PI * swingFrequency * time));
                yield return null;
            }
            SetExtra(0f);
            swing = null;
        }

        // Remember any steady lean already on the layers, so the swing adds to it instead of replacing it.
        void CaptureBase()
        {
            if (swing != null && baseDegrees != null) return;
            baseDegrees = new float[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                if (motionSet.TryGetMotion(elements[i], out LayerMotion motion)) baseDegrees[i] = motion.ExtraDegrees;
        }

        void SetExtra(float degrees)
        {
            for (int i = 0; i < elements.Length; i++)
                if (motionSet.TryGetMotion(elements[i], out LayerMotion motion))
                    motion.ExtraDegrees = (baseDegrees != null ? baseDegrees[i] : 0f) + degrees;
        }

        void SpawnBurst()
        {
            if (burstPrefab == null) return;
            Vector3 at = GetComponent<BoxCollider>().center;
            if (!string.IsNullOrEmpty(burstFrom) && rig.TryGetLayer(burstFrom, out MuralLayer layer))
                at = (Vector3)layer.PivotLocal + burstOffset;
            PaintBurst.Spawn(burstPrefab, experience.transform, at, experience.Data != null ? experience.Data.Palette : null);
        }

        void PlaySound()
        {
            if (sound == null) return;
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.PlayOneShot(sound);
        }
    }
}
