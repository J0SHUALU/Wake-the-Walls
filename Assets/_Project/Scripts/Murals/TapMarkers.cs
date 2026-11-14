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

        [Tooltip("Seconds after the mural is first found before the markers appear, so the intro plays first. Only the first sighting waits.")]
        [SerializeField] float showDelay = 2f;
        [Tooltip("Seconds the markers take to fade in when the mural shows and out when it hides.")]
        [SerializeField] float fadeSeconds = 0.8f;
        [Tooltip("Marker width as a share of the smaller side of the tap area, kept between the limits below.")]
        [SerializeField, Range(0.1f, 0.6f)] float sizeShare = 0.3f;
        [SerializeField] Vector2 sizeLimits = new Vector2(0.12f, 0.35f);
        [SerializeField] float pulseAmount = 0.15f;
        [SerializeField] float pulseSpeed = 1f;

        readonly List<Transform> markers = new List<Transform>();
        readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        float buildAt = -1f;
        bool built;
        bool showing;
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
            if (buildAt < 0f) buildAt = Time.time + showDelay;
            showing = true;
        }

        void HandleLost(MuralData mural)
        {
            if (!IsMine(mural)) return;
            showing = false;
        }

        bool IsMine(MuralData mural) => experience != null && experience.Data == mural;

        void LateUpdate()
        {
            if (buildAt < 0f) return;
            if (!built && Time.time >= buildAt) Build();

            float target = built && showing ? 1f : 0f;
            if (!Mathf.Approximately(alpha, target))
                SetAlpha(Mathf.MoveTowards(alpha, target, Time.deltaTime / Mathf.Max(0.05f, fadeSeconds)));

            if (alpha <= 0f || markers.Count == 0) return;
            float pulse = 1f + pulseAmount * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f));
            float ringWidth = Mathf.Max(0.0001f, ring != null ? ring.bounds.size.x : 1f);
            foreach (Transform marker in markers)
                if (marker != null) marker.GetChild(0).localScale = Vector3.one * (pulse / ringWidth);
        }

        void Build()
        {
            built = true;
            foreach (BoxCollider box in experience.GetComponentsInChildren<BoxCollider>(true))
            {
                var interactable = box.GetComponent<Interactable>();
                if (interactable == null || !DoesSomething(interactable)) continue;

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
            SetAlpha(0f);
        }

        static bool DoesSomething(Interactable interactable)
        {
            if (!interactable.IsInteractable) return false;
            foreach (ITapEffect effect in interactable.GetComponents<ITapEffect>())
                if (effect.HasEffect) return true;
            return false;
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

        void SetAlpha(float value)
        {
            alpha = value;
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null) continue;
                Color c = renderers[i].color;
                c.a = value * (i % 2 == 0 ? 0.9f : 1f);
                renderers[i].color = c;
                renderers[i].enabled = value > 0.002f;
            }
        }
    }
}
