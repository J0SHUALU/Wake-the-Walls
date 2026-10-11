using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Murals
{
    public class TapMarkers : MonoBehaviour
    {
        [SerializeField] MuralExperience experience;
        [SerializeField] Sprite ring;
        [SerializeField] Sprite dot;
        [SerializeField] Color ringColour = new Color(0.9f, 0.16f, 0.13f);
        [SerializeField] Color dotColour = Color.white;

        [Tooltip("Seconds after the mural is found before the markers appear, so the intro plays first.")]
        [SerializeField] float showDelay = 2f;
        [Tooltip("Marker width as a share of the smaller side of the tap area, kept between the limits below.")]
        [SerializeField, Range(0.1f, 0.6f)] float sizeShare = 0.3f;
        [SerializeField] Vector2 sizeLimits = new Vector2(0.12f, 0.35f);
        [SerializeField] float pulseAmount = 0.15f;
        [SerializeField] float pulseSpeed = 1f;

        readonly List<Transform> markers = new List<Transform>();
        readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        Coroutine routine;
        float alpha;

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
            if (!IsMine(mural)) return;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShowAfterDelay(markers.Count == 0 ? showDelay : 0.3f));
        }

        void HandleLost(MuralData mural)
        {
            if (!IsMine(mural)) return;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Tween.Value(alpha, 0f, SetAlpha, 0.6f));
        }

        bool IsMine(MuralData mural) => experience != null && experience.Data == mural;

        IEnumerator ShowAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (markers.Count == 0) Build();
            yield return Tween.Value(alpha, 1f, SetAlpha, 0.8f);
        }

        void Build()
        {
            foreach (var box in experience.GetComponentsInChildren<BoxCollider>(true))
            {
                if (!box.isTrigger || box.GetComponent<Interactable>() == null) continue;

                float side = Mathf.Min(box.size.x * box.transform.lossyScale.x, box.size.y * box.transform.lossyScale.y);
                float width = Mathf.Clamp(side * sizeShare, sizeLimits.x, sizeLimits.y);

                var marker = new GameObject("TapMarker").transform;
                marker.SetParent(box.transform, false);
                marker.localPosition = box.center + new Vector3(0f, 0f, -0.02f);
                marker.localRotation = Quaternion.identity;
                SetWorldWidth(marker, width);

                renderers.Add(AddSprite(marker, ring, ringColour, 0));
                renderers.Add(AddSprite(marker, dot, dotColour, 1, 0.35f));
                markers.Add(marker);
            }
            SetAlpha(alpha);
        }

        static void SetWorldWidth(Transform marker, float width)
        {
            Vector3 parentScale = marker.parent.lossyScale;
            marker.localScale = new Vector3(width / parentScale.x, width / parentScale.y, 1f);
        }

        static SpriteRenderer AddSprite(Transform parent, Sprite sprite, Color colour, int order, float scale = 1f)
        {
            var go = new GameObject(sprite != null ? sprite.name : "Sprite");
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = colour;
            sr.sortingOrder = 500 + order;
            float spriteWidth = sprite != null ? sprite.bounds.size.x : 1f;
            go.transform.localScale = Vector3.one * (scale / spriteWidth);
            return sr;
        }

        void Update()
        {
            if (alpha <= 0f || markers.Count == 0) return;
            float pulse = 1f + pulseAmount * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f));
            foreach (var marker in markers)
            {
                if (marker != null) marker.GetChild(0).localScale = Vector3.one * (pulse / Mathf.Max(0.0001f, ring != null ? ring.bounds.size.x : 1f));
            }
        }

        void SetAlpha(float value)
        {
            alpha = value;
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null) continue;
                Color c = renderers[i].color;
                c.a = value * (i % 2 == 0 ? 0.9f : 1f);
                renderers[i].color = c;
            }
        }
    }
}
