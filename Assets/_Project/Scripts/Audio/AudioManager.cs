using System;
using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Audio
{
    /// <summary>
    /// Plays each mural's ambience. It starts on MuralFound, crossfades when a different mural is found,
    /// fades out and pauses on MuralLost, and fades back in from the same spot when the mural is found again.
    /// Leaving the experience for Start or Scanning fades it out. Uses two looping sources for the crossfade.
    /// </summary>
    public partial class AudioManager : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float ambienceVolume = 0.7f;
        [Tooltip("Seconds to crossfade between two murals.")]
        [SerializeField] float crossfadeDuration = 1.5f;
        [Tooltip("Seconds to fade out when the mural is lost, and back in when it is found again.")]
        [SerializeField] float fadeDuration = 0.8f;

        static bool muted;

        /// <summary>True while all app sound is muted. The choice lasts for the session.</summary>
        public static bool IsMuted => muted;

        /// <summary>Raised with the new state whenever sound is muted or unmuted.</summary>
        public static event Action<bool> MuteChanged;

        AudioSource[] sources;
        Coroutine[] fades;
        int active;

        /// <summary>The ambience clip currently playing or paused, if any.</summary>
        public AudioClip CurrentAmbience => sources[active].clip;

        void Awake()
        {
            AudioListener.volume = muted ? 0f : 1f;
            sources = new[] { CreateSource("Ambience A"), CreateSource("Ambience B") };
            fades = new Coroutine[2];
            SetUpEffects();
        }

        void OnEnable()
        {
            GameEvents.MuralFound += HandleMuralFound;
            GameEvents.MuralLost += HandleMuralLost;
            GameEvents.StateChanged += HandleStateChanged;
            GameEvents.InteractableTapped += PlayTap;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleMuralFound;
            GameEvents.MuralLost -= HandleMuralLost;
            GameEvents.StateChanged -= HandleStateChanged;
            GameEvents.InteractableTapped -= PlayTap;
        }

        /// <summary>Mutes or unmutes every sound in the app, including the murals' own effects.</summary>
        public static void SetMuted(bool value)
        {
            muted = value;
            AudioListener.volume = muted ? 0f : 1f;
            MuteChanged?.Invoke(muted);
        }

        /// <summary>Flips mute on or off. Wired to the HUD Sound button.</summary>
        public void ToggleMute() => SetMuted(!muted);

        // A fresh app launch always starts with sound on.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetMute()
        {
            muted = false;
            MuteChanged = null;
            AudioListener.volume = 1f;
        }

        void HandleMuralFound(MuralData mural)
        {
            AudioClip clip = mural != null ? mural.Ambience : null;
            AudioSource current = sources[active];

            if (clip != null && clip == current.clip)
            {
                // Same mural found again: carry on from where it paused.
                FadeTo(active, ambienceVolume, fadeDuration, false);
                return;
            }

            // A different mural (or none): fade the old one out and the new one in at the same time.
            FadeTo(active, 0f, crossfadeDuration, true);
            if (clip == null) return;

            active = 1 - active;
            AudioSource next = sources[active];
            next.clip = clip;
            next.volume = 0f;
            next.time = 0f;
            FadeTo(active, ambienceVolume, crossfadeDuration, false);
        }

        void HandleMuralLost(MuralData mural) => FadeTo(active, 0f, fadeDuration, false);

        void HandleStateChanged(AppState state)
        {
            if (state == AppState.Start || state == AppState.Scanning) FadeTo(active, 0f, fadeDuration, true);
        }

        // Fades one source to a volume. At zero it pauses, or stops and forgets its clip when told to.
        void FadeTo(int index, float target, float duration, bool stopWhenSilent)
        {
            AudioSource source = sources[index];
            if (fades[index] != null) StopCoroutine(fades[index]);
            if (source.clip == null) return;
            if (target > 0f && !source.isPlaying) source.UnPause();
            if (target > 0f && !source.isPlaying) source.Play();
            fades[index] = StartCoroutine(Fade(index, target, duration, stopWhenSilent));
        }

        IEnumerator Fade(int index, float target, float duration, bool stopWhenSilent)
        {
            AudioSource source = sources[index];
            yield return Tween.Value(source.volume, target, v => source.volume = v, duration, Tween.EaseInOutCubic);
            if (target <= 0f)
            {
                if (stopWhenSilent)
                {
                    source.Stop();
                    source.clip = null;
                }
                else
                {
                    source.Pause();
                }
            }
            fades[index] = null;
        }

        AudioSource CreateSource(string sourceName)
        {
            var child = new GameObject(sourceName);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }
    }
}
