using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Releases small butterflies in the mural's colours from chosen elements. Each one flaps out of
    /// the wall, wanders towards the viewer and fades away. The wing is drawn in code, so no art is needed.
    /// Which elements they leave from is set on the prefab, so the script holds no mural names.
    /// </summary>
    public class Butterflies : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] string[] fromElements = new string[0];
        [Tooltip("Wing colours. Uses the mural palette when empty.")]
        [SerializeField] Color[] colours = new Color[0];
        [SerializeField] float startDelay = 4f;
        [SerializeField] Vector2 interval = new Vector2(1.5f, 3.5f);
        [SerializeField] float lifeSeconds = 7f;
        [Tooltip("Wingspan in metres.")]
        [SerializeField] float wingspan = 0.16f;
        [SerializeField] float flapsPerSecond = 6f;
        [Tooltip("How far towards the viewer they wander, in metres.")]
        [SerializeField] float outDistance = 1.2f;

        static Sprite wing;
        Transform flock;
        Coroutine loop;

        void OnEnable()
        {
            GameEvents.MuralFound += HandleFound;
            GameEvents.MuralLost += HandleLost;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleFound;
            GameEvents.MuralLost -= HandleLost;
        }

        void HandleFound(MuralData mural)
        {
            if (experience == null || experience.Data != mural || fromElements.Length == 0) return;
            if (loop != null) StopCoroutine(loop);
            loop = StartCoroutine(Release());
        }

        void HandleLost(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            StopAllCoroutines();
            loop = null;
            if (flock != null) Destroy(flock.gameObject);
        }

        IEnumerator Release()
        {
            yield return new WaitForSeconds(startDelay);
            while (experience.IsRunning)
            {
                string element = fromElements[Random.Range(0, fromElements.Length)];
                if (experience.Rig.TryGetLayer(element, out var layer)) StartCoroutine(Fly(layer.PivotLocal));
                yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
            }
        }

        IEnumerator Fly(Vector2 from)
        {
            Color[] palette = colours.Length > 0 ? colours : experience.Data.Palette;
            Color colour = palette.Length > 0 ? palette[Random.Range(0, palette.Length)] : Color.white;
            if (flock == null)
            {
                flock = new GameObject("Butterflies").transform;
                flock.SetParent(experience.transform, false);
            }
            var body = new GameObject("Butterfly").transform;
            body.SetParent(flock, false);
            var right = AddWing(body, colour, 0f);
            var left = AddWing(body, colour, 180f);
            body.localScale = Vector3.one * wingspan * 0.5f;

            Vector2 size = experience.Rig.MuralSize;
            Vector3 start = new Vector3(from.x, from.y, -0.05f);
            Vector3 end = new Vector3(Random.Range(-0.5f, 0.5f) * size.x, Random.Range(0f, 0.6f) * size.y, -outDistance * Random.Range(0.6f, 1f));
            Vector3 bend = Vector3.Lerp(start, end, 0.5f) + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(0.1f, 0.4f), 0f);
            float seed = Random.value * 10f;

            for (float time = 0f; time < lifeSeconds && body != null; time += Time.deltaTime)
            {
                float t = time / lifeSeconds;
                Vector3 path = Vector3.Lerp(Vector3.Lerp(start, bend, t), Vector3.Lerp(bend, end, t), t);
                Vector3 flutter = new Vector3(Mathf.Sin(seed + time * 2.3f), Mathf.Sin(seed * 2f + time * 3.1f), 0f) * 0.05f;
                body.localPosition = path + flutter;
                body.localRotation = Quaternion.Euler(-20f, 25f * Mathf.Sin(seed + time), 15f * Mathf.Sin(seed + time * 1.7f));

                float flap = 75f * Mathf.Abs(Mathf.Sin(time * flapsPerSecond * Mathf.PI));
                right.localRotation = Quaternion.Euler(0f, flap, 0f);
                left.localRotation = Quaternion.Euler(0f, 180f - flap, 0f);

                float a = Mathf.Min(1f, t / 0.1f, (1f - t) / 0.2f);
                foreach (var sr in body.GetComponentsInChildren<SpriteRenderer>()) sr.color = new Color(colour.r, colour.g, colour.b, a);
                yield return null;
            }
            if (body != null) Destroy(body.gameObject);
        }

        static Transform AddWing(Transform body, Color colour, float yaw)
        {
            var go = new GameObject("Wing");
            go.transform.SetParent(body, false);
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Wing();
            sr.color = colour;
            sr.sortingOrder = 520;
            return go.transform;
        }

        // A wing made of a large upper lobe and a smaller lower lobe, with a dark edge and a pale spot.
        static Sprite Wing()
        {
            if (wing != null) return wing;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size, v = y / (float)size;
                float upper = Lobe(u, v, 0.45f, 0.68f, 0.45f, 0.3f);
                float lower = Lobe(u, v, 0.32f, 0.3f, 0.3f, 0.22f);
                float inside = Mathf.Max(upper, lower);
                float edge = Mathf.Clamp01((0.25f - inside) * 4f);
                float spot = Lobe(u, v, 0.7f, 0.75f, 0.08f, 0.07f) > 0f ? 1f : 0f;
                float shade = Mathf.Lerp(1f, 0.15f, edge);
                Color c = Color.Lerp(new Color(shade, shade, shade), Color.white, spot * 0.8f);
                c.a = Mathf.Clamp01(inside * 6f);
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            wing = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0f, 0.5f), size);
            return wing;
        }

        // 1 at the centre of an ellipse, falling to 0 at its edge and below 0 outside.
        static float Lobe(float u, float v, float cx, float cy, float rx, float ry)
        {
            float dx = (u - cx) / rx, dy = (v - cy) / ry;
            return 1f - Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
