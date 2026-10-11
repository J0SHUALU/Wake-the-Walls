using UnityEngine;
using UnityEngine.UI;

namespace WakeTheWalls.Audio
{
    public partial class AudioManager
    {
        [Header("Sound effects")]
        [Tooltip("Played on every UI button press in the canvas this manager sits in.")]
        [SerializeField] AudioClip buttonClip;
        [Tooltip("Played when something tappable in a mural is tapped.")]
        [SerializeField] AudioClip tapClip;
        [SerializeField, Range(0f, 1f)] float effectsVolume = 0.6f;

        AudioSource effects;

        public void PlayButton() => PlayEffect(buttonClip);

        public void PlayTap() => PlayEffect(tapClip);

        void SetUpEffects()
        {
            effects = CreateSource("Effects");
            effects.loop = false;
            effects.volume = 1f;

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
