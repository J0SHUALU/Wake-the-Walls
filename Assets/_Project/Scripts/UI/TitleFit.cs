using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WakeTheWalls.UI
{
    public static class TitleFit
    {
        const float MinShare = 0.5f;
        const float Step = 0.95f;

        static readonly Dictionary<TMP_Text, float> baseSizes = new Dictionary<TMP_Text, float>();
        static readonly Vector3[] corners = new Vector3[4];

        public static void FitBefore(TMP_Text title, RectTransform limit, float gap = 48f)
        {
            if (title == null) return;
            float size = BaseSize(title);
            title.fontSize = size;
            if (limit == null || !title.gameObject.activeInHierarchy) return;

            limit.GetWorldCorners(corners);
            float stop = corners[0].x - gap * title.transform.lossyScale.x;
            while (title.fontSize > size * MinShare && RightEdge(title) > stop) title.fontSize *= Step;
            Rebuild(title);
        }

        public static void Wrap(TMP_Text title)
        {
            if (title == null) return;
            float size = BaseSize(title);
            title.textWrappingMode = TextWrappingModes.Normal;
            title.enableAutoSizing = true;
            title.fontSizeMax = size;
            title.fontSizeMin = size * MinShare;
        }

        static float BaseSize(TMP_Text title)
        {
            if (!baseSizes.TryGetValue(title, out float size))
            {
                size = title.fontSize;
                baseSizes[title] = size;
            }
            return size;
        }

        static float RightEdge(TMP_Text title)
        {
            var rect = title.rectTransform;
            rect.GetWorldCorners(corners);
            float width = title.GetPreferredValues(title.text, float.PositiveInfinity, float.PositiveInfinity).x;
            return corners[0].x + width * rect.lossyScale.x;
        }

        static void Rebuild(TMP_Text title)
        {
            var parent = title.rectTransform.parent as RectTransform;
            if (parent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }
    }
}
