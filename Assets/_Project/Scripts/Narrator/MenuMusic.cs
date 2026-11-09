using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Narrator
{
    [RequireComponent(typeof(AudioSource))]
    public class MenuMusic : MonoBehaviour
    {
        [SerializeField] AudioClip clip;
        [SerializeField, Range(0f, 1f)] float volume = 0.55f;
        [SerializeField] float fadeDuration = 1.2f;

        AudioSource source;
        Coroutine fading;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }

        void OnEnable() => GameEvents.StateChanged += HandleState;

        void OnDisable() => GameEvents.StateChanged -= HandleState;

        void HandleState(AppState state)
        {
            bool menu = state == AppState.Start;
            if (menu && !source.isPlaying && clip != null) source.Play();
            FadeTo(menu ? volume : 0f);
        }

        void FadeTo(float to)
        {
            if (fading != null) StopCoroutine(fading);
            fading = StartCoroutine(Fade(to));
        }

        IEnumerator Fade(float to)
        {
            yield return Tween.Value(source.volume, to, v => source.volume = v, fadeDuration, Tween.EaseInOutCubic);
            if (to <= 0f) source.Pause();
            fading = null;
        }
    }
}
