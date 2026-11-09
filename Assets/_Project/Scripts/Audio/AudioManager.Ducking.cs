using UnityEngine;

namespace WakeTheWalls.Audio
{
    public partial class AudioManager
    {
        [Header("Ducking")]
        [Tooltip("Music level while the narrator is talking.")]
        [SerializeField, Range(0f, 1f)] float duckedLevel = 0.35f;
        [SerializeField] float duckSpeed = 4f;

        static bool ducked;

        readonly float[] levels = new float[2];
        float duck = 1f;

        public static void SetDucked(bool value) => ducked = value;

        void SetLevel(int index, float value)
        {
            levels[index] = value;
            sources[index].volume = value * duck;
        }

        void Update()
        {
            float goal = ducked ? duckedLevel : 1f;
            if (Mathf.Approximately(duck, goal)) return;
            duck = Mathf.MoveTowards(duck, goal, duckSpeed * Time.unscaledDeltaTime);
            for (int i = 0; i < sources.Length; i++) sources[i].volume = levels[i] * duck;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetDuck() => ducked = false;
    }
}
