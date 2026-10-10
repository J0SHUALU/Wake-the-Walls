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
    public class AudioManager : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float ambienceVolume = 0.7f;
        [Tooltip("Seconds to crossfade between two murals.")]
        [SerializeField] float crossfadeDuration = 1.5f;
        [Tooltip("Seconds to fade out when the mural is lost, and back in when it is found again.")]
        [SerializeField] float fadeDuration = 0.8f;

        AudioSource[] sources;
        Coroutine[] fades;
        int active;

        /// <summary>The ambience clip currently playing or paused, if any.</summary>
        public AudioClip CurrentAmbience => sources[active].clip;

        void Awake()
        {
            sources = new[] { CreateSource("Ambience A"), CreateSource("Ambience B") };
            fades = new Coroutine[2];
        }

        void OnEnable()
        {
            GameEvents.MuralFound += HandleMuralFound;
            GameEvents.MuralLost += HandleMuralLost;
            GameEvents.StateChanged += HandleStateChanged;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleMuralFound;
            GameEvents.MuralLost -= HandleMuralLost;
            GameEvents.StateChanged -= HandleStateChanged;
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
