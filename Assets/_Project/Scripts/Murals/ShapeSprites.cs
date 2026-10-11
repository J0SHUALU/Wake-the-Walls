using System;
using UnityEngine;

namespace WakeTheWalls.Murals
{
    public static class ShapeSprites
    {
        public enum Shape { ButterflyWing, BirdWing, Leaf, Petal, Speck }

        const int Size = 64;
        static readonly Sprite[] cache = new Sprite[5];

        public static Sprite Get(Shape shape)
        {
            int i = (int)shape;
            if (cache[i] != null) return cache[i];
            switch (shape)
            {
                case Shape.ButterflyWing: cache[i] = Draw(ButterflyWing, new Vector2(0f, 0.5f)); break;
                case Shape.BirdWing: cache[i] = Draw(BirdWing, new Vector2(0f, 0.5f)); break;
                case Shape.Leaf: cache[i] = Draw(Leaf, new Vector2(0.5f, 0.5f)); break;
                case Shape.Petal: cache[i] = Draw(Petal, new Vector2(0.5f, 0.5f)); break;
                default: cache[i] = Draw(Speck, new Vector2(0.5f, 0.5f)); break;
            }
            return cache[i];
        }

        static Sprite Draw(Func<float, float, Color> pixel, Vector2 pivot)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    tex.SetPixel(x, y, pixel((x + 0.5f) / Size, (y + 0.5f) / Size));
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), pivot, Size);
        }

        static Color ButterflyWing(float u, float v)
        {
            float inside = Mathf.Max(Lobe(u, v, 0.45f, 0.68f, 0.45f, 0.3f), Lobe(u, v, 0.32f, 0.3f, 0.3f, 0.22f));
            float shade = Mathf.Lerp(1f, 0.15f, Mathf.Clamp01((0.25f - inside) * 4f));
            Color c = Color.Lerp(new Color(shade, shade, shade), Color.white, Lobe(u, v, 0.7f, 0.75f, 0.08f, 0.07f) > 0f ? 0.8f : 0f);
            c.a = Mathf.Clamp01(inside * 6f);
            return c;
        }

        static Color BirdWing(float u, float v)
        {
            float centre = 0.5f + 0.18f * u * u;
            float half = 0.16f * (1f - u) + 0.02f;
            float inside = 1f - Mathf.Abs(v - centre) / half;
            return new Color(1f, 1f, 1f, Mathf.Clamp01(inside * 4f) * (u < 0.98f ? 1f : 0f));
        }

        static Color Leaf(float u, float v)
        {
            float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;
            float width = 0.45f * (1f - x * x);
            float inside = 1f - Mathf.Abs(y) / Mathf.Max(0.001f, width);
            float rib = Mathf.Abs(y) < 0.03f && Mathf.Abs(x) < 0.85f ? 0.3f : 0f;
            Color c = Color.Lerp(Color.white * 0.85f, Color.white, rib);
            c.a = Mathf.Clamp01(inside * 5f) * (Mathf.Abs(x) < 1f ? 1f : 0f);
            return c;
        }

        static Color Petal(float u, float v)
        {
            float width = 0.42f * Mathf.Sqrt(Mathf.Clamp01(v)) * (1f - 0.5f * v * v);
            float inside = Mathf.Min(1f - Mathf.Abs(u - 0.5f) / Mathf.Max(0.001f, width), Lobe(u, v, 0.5f, 0.5f, 0.5f, 0.5f) * 4f);
            Color c = Color.Lerp(Color.white * 0.8f, Color.white, v);
            c.a = Mathf.Clamp01(inside * 5f);
            return c;
        }

        static Color Speck(float u, float v) => new Color(1f, 1f, 1f, Mathf.Clamp01(Lobe(u, v, 0.5f, 0.5f, 0.5f, 0.5f) * 2f));

        static float Lobe(float u, float v, float cx, float cy, float rx, float ry)
        {
            float dx = (u - cx) / rx, dy = (v - cy) / ry;
            return 1f - Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
