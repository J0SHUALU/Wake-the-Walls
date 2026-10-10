using UnityEngine;
using UnityEngine.UI;

namespace WakeTheWalls.Audio
{
    /// <summary>
    /// Short sound effects played through the AudioManager: a soft click for every UI button press,
    /// and a tap sound whenever something tappable in a mural is tapped (GameEvents.InteractableTapped).
    /// Muting through <see cref="AudioManager.SetMuted"/> silences these too.
    /// </summary>
    public partial class AudioManager
    {
        [Header("Sound effects")]
        [Tooltip("Played on every UI button press in the canvas this manager sits in.")]
        [SerializeField] AudioClip buttonClip;
        [Tooltip("Played when something tappable in a mural is tapped.")]
        [SerializeField] AudioClip tapClip;
        [SerializeField, Range(0f, 1f)] float effectsVolume = 0.6f;

        AudioSource effects;

        /// <summary>Plays the UI button press sound.</summary>
        public void PlayButton() => PlayEffect(buttonClip);

        /// <summary>Plays the sound for a tap on something in a mural.</summary>
        public void PlayTap() => PlayEffect(tapClip);

        void SetUpEffects()
        {
            effects = CreateSource("Effects");
            effects.loop = false;
            effects.volume = 1f;

            // Every button in the UI gets the press sound, so screens never need wiring one by one.
            Transform canvas = transform.parent != null ? transform.parent : transform;
            foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
            {
                button.onClick.AddListener(PlayButton);
            }
        }

        void PlayEffect(AudioClip clip)
        {
            if (clip != null && effects != null) effects.PlayOneShot(clip, effectsVolume);
        }
    }
}
