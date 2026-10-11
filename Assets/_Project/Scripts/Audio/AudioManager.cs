using System;
using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Audio
{
    public partial class AudioManager : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float ambienceVolume = 0.7f;
        [Tooltip("Seconds to crossfade between two murals.")]
        [SerializeField] float crossfadeDuration = 1.5f;
        [Tooltip("Seconds to fade out when the mural is lost, and back in when it is found again.")]
        [SerializeField] float fadeDuration = 0.8f;

        static bool muted;

        public static bool IsMuted => muted;

        public static event Action<bool> MuteChanged;

        AudioSource[] sources;
        Coroutine[] fades;
        int active;

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

        public static void SetMuted(bool value)
        {
            muted = value;
            AudioListener.volume = muted ? 0f : 1f;
            MuteChanged?.Invoke(muted);
        }

        public void ToggleMute() => SetMuted(!muted);

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
                FadeTo(active, ambienceVolume, fadeDuration, false);
                return;
            }

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
