using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// The row of wall stamps, one per mural in the library, collected like a passport.
    /// Empty stamps are a dashed ring, woken ones are a tilted brick disc with the number.
    /// The stamps are built at runtime from the library, so the mural count is never hard coded.
    /// </summary>
    public class StampsRow : MonoBehaviour
    {
        [SerializeField] MuralLibrary library;

        [Header("Look")]
        [SerializeField] Sprite emptySprite;
        [SerializeField] Sprite filledSprite;
        [SerializeField] TMP_FontAsset numberFont;
        [SerializeField] float stampSize = 90f;
        [SerializeField] float numberSize = 48f;
        [SerializeField] float filledTilt = -6f;
        [SerializeField] Color emptyColor = new Color32(0x5E, 0x55, 0x4B, 0xFF);
        [SerializeField] Color filledColor = new Color32(0xA9, 0x48, 0x1F, 0xFF);
        [SerializeField] Color filledNumberColor = Color.white;

        [Header("Count")]
        [Tooltip("Optional label after the stamps, for example \"0 of 5 awake\".")]
        [SerializeField] TMP_Text countLabel;
        [SerializeField] string countFormat = "{0} of {1} awake";

        readonly List<(Image ring, TMP_Text number, MuralData mural)> stamps = new List<(Image, TMP_Text, MuralData)>();

        void Awake() => Build();

        /// <summary>Fills the stamps for the murals in <paramref name="woken"/> and updates the count.</summary>
        public void Show(ICollection<MuralData> woken)
        {
            if (stamps.Count == 0) Build();

            int count = 0;
            foreach (var stamp in stamps)
            {
                bool isWoken = woken != null && woken.Contains(stamp.mural);
                if (isWoken) count++;
                stamp.ring.sprite = isWoken ? filledSprite : emptySprite;
                stamp.ring.color = isWoken ? filledColor : emptyColor;
                stamp.number.color = isWoken ? filledNumberColor : emptyColor;
                stamp.ring.rectTransform.localRotation = Quaternion.Euler(0f, 0f, isWoken ? filledTilt : 0f);
            }

            if (countLabel != null) countLabel.text = string.Format(countFormat, count, stamps.Count);
        }

        void Build()
        {
            if (library == null || stamps.Count > 0) return;

            for (int i = 0; i < library.Murals.Count; i++)
            {
                var stampObject = new GameObject($"Stamp {i + 1}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                stampObject.transform.SetParent(transform, false);
                if (countLabel != null) stampObject.transform.SetSiblingIndex(countLabel.transform.GetSiblingIndex());

                var layout = stampObject.GetComponent<LayoutElement>();
                layout.preferredWidth = stampSize;
                layout.preferredHeight = stampSize;

                var ring = stampObject.GetComponent<Image>();
                ring.raycastTarget = false;
                ring.preserveAspect = true;

                var numberObject = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
                numberObject.transform.SetParent(stampObject.transform, false);
                var numberRect = numberObject.GetComponent<RectTransform>();
                numberRect.anchorMin = Vector2.zero;
                numberRect.anchorMax = Vector2.one;
                numberRect.offsetMin = Vector2.zero;
                numberRect.offsetMax = Vector2.zero;

                var number = numberObject.GetComponent<TextMeshProUGUI>();
                number.text = (i + 1).ToString("00");
                number.font = numberFont;
                number.fontSize = numberSize;
                number.alignment = TextAlignmentOptions.Center;
                number.raycastTarget = false;

                stamps.Add((ring, number, library.Murals[i]));
            }

            Show(null);
        }
    }
}
