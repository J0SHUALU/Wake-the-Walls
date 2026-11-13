using System.Collections.Generic;
using UnityEngine;

namespace WakeTheWalls.Murals
{
    // Soft smoke that curls up from points on the mural, for chimneys and cooking fires.
    public class SmokePlume : MonoBehaviour
    {
        class Puff { public Transform transform; public SpriteRenderer sprite; public float age, life, phase, size; }

        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Where smoke rises from, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Vector2[] sources = new Vector2[0];
        [SerializeField] Color colour = new Color(0.93f, 0.91f, 0.88f, 1f);
        [SerializeField, Range(0.05f, 0.6f)] float opacity = 0.3f;
        [SerializeField] float puffsPerSecond = 2.5f;
        [Tooltip("Size of a puff when it starts and when it fades, in metres.")]
        [SerializeField] Vector2 size = new Vector2(0.05f, 0.22f);
        [SerializeField] float riseSpeed = 0.1f;
        [Tooltip("Sideways drift in metres per second, for a light wind.")]
        [SerializeField] float wind = 0.025f;
        [SerializeField] Vector2 lifeSeconds = new Vector2(3.5f, 5f);
        [SerializeField] float startDelay = 2f;

        static Sprite soft;
        readonly List<Puff> live = new List<Puff>();
        float nextPuff, runningFor;

        void Update()
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            runningFor = experience.IsRunning ? runningFor + Time.deltaTime : 0f;
            if (experience.IsRunning && runningFor > startDelay && sources.Length > 0 && Time.time >= nextPuff)
            {
                foreach (Vector2 source in sources) Spawn(source);
                nextPuff = Time.time + 1f / Mathf.Max(0.1f, puffsPerSecond);
            }
            Move();
        }

        void Spawn(Vector2 source)
        {
            Vector2 mural = experience.Rig.MuralSize;
            var go = new GameObject("Smoke");
            go.transform.SetParent(experience.transform, false);
            go.transform.localPosition = new Vector3((source.x - 0.5f) * mural.x + Random.Range(-0.01f, 0.01f), (source.y - 0.5f) * mural.y, -0.02f);
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = SoftSprite();
            sprite.color = new Color(colour.r, colour.g, colour.b, 0f);
            sprite.sortingOrder = 520;
            live.Add(new Puff { transform = go.transform, sprite = sprite, phase = Random.value * 10f, life = Random.Range(lifeSeconds.x, lifeSeconds.y), size = Random.Range(0.8f, 1.2f) });
        }

        void Move()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                Puff puff = live[i];
                puff.age += Time.deltaTime;
                if (puff.transform == null || puff.age >= puff.life || !experience.IsRunning && puff.sprite.color.a <= 0.01f)
                {
                    if (puff.transform != null) Destroy(puff.transform.gameObject);
                    live.RemoveAt(i);
                    continue;
                }
                float t = puff.age / puff.life;
                float curl = Mathf.Sin(puff.phase + puff.age * 1.3f) * 0.02f;
                puff.transform.localPosition += new Vector3(wind + curl, riseSpeed, 0f) * Time.deltaTime;
                puff.transform.localScale = Vector3.one * Mathf.Lerp(size.x, size.y, t) * puff.size;
                float fade = Mathf.Min(t / 0.15f, 1f) * Mathf.Min((1f - t) / 0.5f, 1f);
                if (!experience.IsRunning) fade *= Mathf.Max(0f, 1f - puff.age);
                Color c = puff.sprite.color;
                c.a = opacity * fade;
                puff.sprite.color = c;
            }
        }

        static Sprite SoftSprite()
        {
            if (soft != null) return soft;
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float a = Mathf.Clamp01(1f - d);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
                }
            tex.Apply();
            soft = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            return soft;
        }
    }
}
