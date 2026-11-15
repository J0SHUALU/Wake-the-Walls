using System.Collections;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
    // A gold ring that draws itself around a painted face, with a soft light resting inside it. Stays flat on the wall.
    public class FaceRing : MonoBehaviour
    {
        const int Points = 72;
        static Texture2D glowTexture;
        static Material sharedMaterial;

        LineRenderer line;
        SpriteRenderer glow;
        float radius, alpha;
        Coroutine draw;
        Color colour;

        public bool IsShown { get; private set; }

        public static FaceRing Create(Transform parent, Vector3 centre, float radius, float lineWidth, Color colour)
        {
            var ring = new GameObject("FaceRing").AddComponent<FaceRing>();
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = centre;
            ring.Build(radius, lineWidth, colour);
            return ring;
        }

        void Build(float r, float lineWidth, Color c)
        {
            radius = r;
            colour = c;
            if (sharedMaterial == null) sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            glow = new GameObject("Light").AddComponent<SpriteRenderer>();
            glow.transform.SetParent(transform, false);
            glow.sprite = Sprite.Create(GlowTexture(), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128f);
            glow.transform.localScale = Vector3.one * radius * 2.2f;
            glow.sortingOrder = 400;

            line = gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = false;
            line.sharedMaterial = sharedMaterial;
            line.widthMultiplier = lineWidth;
            line.numCapVertices = 4;
            line.sortingOrder = 401;
            line.positionCount = 0;
            SetAlpha(0f);
        }

        public void Show(float seconds = 0.8f)
        {
            if (draw != null) StopCoroutine(draw);
            IsShown = true;
            draw = StartCoroutine(Draw(seconds));
        }

        public void Hide(float seconds = 0.5f)
        {
            if (draw != null) StopCoroutine(draw);
            IsShown = false;
            draw = isActiveAndEnabled ? StartCoroutine(Tween.Value(alpha, 0f, SetAlpha, seconds)) : null;
            if (!isActiveAndEnabled) SetAlpha(0f);
        }

        public void ResetRing()
        {
            if (draw != null) StopCoroutine(draw);
            draw = null;
            IsShown = false;
            line.positionCount = 0;
            SetAlpha(0f);
        }

        void Update()
        {
            if (!IsShown) return;
            float breathe = 0.8f + 0.2f * Mathf.Sin(Time.time * 2f + transform.position.x * 10f);
            Color g = colour;
            g.a = alpha * 0.35f * breathe;
            glow.color = g;
        }

        IEnumerator Draw(float seconds)
        {
            SetAlpha(1f);
            yield return Tween.Run(seconds, t =>
            {
                int count = Mathf.Max(2, Mathf.RoundToInt(Points * t) + 1);
                line.positionCount = count;
                for (int i = 0; i < count; i++)
                {
                    float a = Mathf.PI * 0.5f - i / (float)Points * Mathf.PI * 2f;
                    line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
                }
            }, Tween.EaseOutCubic);
            draw = null;
        }

        void SetAlpha(float value)
        {
            alpha = value;
            Color c = colour;
            c.a = value;
            line.startColor = line.endColor = c;
            Color g = colour;
            g.a = value * 0.35f;
            glow.color = g;
        }

        static Texture2D GlowTexture()
        {
            if (glowTexture != null) return glowTexture;
            glowTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f)) / 63.5f;
                float a = Mathf.Clamp01(1f - d);
                glowTexture.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
            glowTexture.Apply();
            return glowTexture;
        }
    }
}
