using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WakeTheWalls.UI
{
    // The wall labels size themselves to their text, so a long mural title would run off the screen.
    // This caps the title's width and lets it wrap onto a second line instead.
    public static class TitleFit
    {
        public static void Set(TMP_Text target, string text, float maxWidth)
        {
            target.text = text;
            target.textWrappingMode = TextWrappingModes.Normal;

            var element = target.GetComponent<LayoutElement>();
            if (element == null) element = target.gameObject.AddComponent<LayoutElement>();

            float oneLine = target.GetPreferredValues(text, Mathf.Infinity, Mathf.Infinity).x;
            element.preferredWidth = oneLine > maxWidth ? maxWidth : -1f;
        }
    }
}
