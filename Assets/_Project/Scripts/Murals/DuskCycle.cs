using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    // Slowly warms the whole mural into dusk and back, with optional embers drifting up while it's dark.
    public class DuskCycle : MonoBehaviour
    {
        static readonly int ColourId = Shader.PropertyToID("_BaseColor");

        class Ember { public Transform transform; public SpriteRenderer sprite; public float age, life, phase; }

        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Seconds for one full day to dusk and back.")]
        [SerializeField, Min(10f)] float cycleSeconds = 36f;
        [Tooltip("Seconds of daylight before the first dusk.")]
        [SerializeField, Min(0f)] float firstDusk = 10f;
        [Tooltip("Colour the mural is multiplied by at the deepest point of dusk.")]
        [SerializeField] Color duskTint = new Color(0.86f, 0.66f, 0.55f, 1f);

        [Header("Embers")]
        [SerializeField] bool embers;
        [Tooltip("Where embers rise from, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect emberArea = new Rect(0.1f, 0.25f, 0.8f, 0.4f);
        [SerializeField] float embersPerSecond = 6f;
        [SerializeField] Color emberColour = new Color(1f, 0.62f, 0.2f, 1f);
        [SerializeField] float emberSize = 0.012f;

        static Sprite dot;
        readonly List<Ember> live = new List<Ember>();
        MaterialPropertyBlock block;
        float time, weight, nextEmber;

        public float Dusk { get; private set; }

        void Update()
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            weight = Mathf.MoveTowards(weight, experience.IsRunning ? 1f : 0f, Time.deltaTime / 1.5f);
            if (experience.IsRunning) time += Time.deltaTime;

            float phase = Mathf.Max(0f, time - firstDusk) / cycleSeconds;
            Dusk = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Repeat(phase, 1f)), 2f) * weight * (time > firstDusk ? 1f : 0f);
            Tint(Color.Lerp(Color.white, duskTint, Dusk));

            if (embers && experience.IsRunning && Dusk > 0.35f && Time.time >= nextEmber)
            {
                Spawn();
                nextEmber = Time.time + 1f / Mathf.Max(0.1f, embersPerSecond * Dusk);
            }
            MoveEmbers();
        }

        void OnDisable() => Tint(Color.white);

        void Tint(Color colour)
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            block ??= new MaterialPropertyBlock();
            Apply(experience.Rig.Background, colour);
            foreach (MuralLayer layer in experience.Rig.Layers) Apply(layer, colour);
        }

        void Apply(MuralLayer layer, Color colour)
        {
            if (layer == null || layer.Renderer == null) return;
            layer.Renderer.GetPropertyBlock(block);
            block.SetColor(ColourId, colour);
            layer.Renderer.SetPropertyBlock(block);
        }

        void Spawn()
        {
            Vector2 mural = experience.Rig.MuralSize;
            var go = new GameObject("Ember");
            go.transform.SetParent(experience.transform, false);
            go.transform.localPosition = new Vector3((emberArea.x + Random.value * emberArea.width - 0.5f) * mural.x,
                (emberArea.y + Random.value * emberArea.height - 0.5f) * mural.y, -0.03f);
            go.transform.localScale = Vector3.one * emberSize * Random.Range(0.7f, 1.4f);
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = Dot();
            sprite.color = new Color(emberColour.r, emberColour.g, emberColour.b, 0f);
            sprite.sortingOrder = 525;
            live.Add(new Ember { transform = go.transform, sprite = sprite, life = Random.Range(2.5f, 4f), phase = Random.value * 10f });
        }

        void MoveEmbers()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                Ember e = live[i];
                e.age += Time.deltaTime;
                if (e.transform == null || e.age >= e.life || !experience.IsRunning)
                {
                    if (e.transform != null) Destroy(e.transform.gameObject);
                    live.RemoveAt(i);
                    continue;
                }
                float t = e.age / e.life;
                e.transform.localPosition += new Vector3(Mathf.Sin(e.phase + e.age * 3f) * 0.03f, 0.09f, 0f) * Time.deltaTime;
                Color c = e.sprite.color;
                c.a = Mathf.Sin(t * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(e.phase + e.age * 12f));
                e.sprite.color = c;
            }
        }

        static Sprite Dot()
        {
            if (dot != null) return dot;
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d)));
                }
            tex.Apply();
            dot = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            return dot;
        }
    }
}
