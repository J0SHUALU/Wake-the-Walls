using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Interaction
{
    public class InfoHotspot : Interactable
    {
        [Serializable]
        public class Topic
        {
            public string id;
            public string title;
            [TextArea(2, 5)] public string text;
        }

        [Header("Content")]
        [SerializeField] string title;
        [TextArea(2, 5)]
        [SerializeField] string text;
        [Tooltip("Optional extra topics, opened by id through Show(topicId).")]
        [SerializeField] List<Topic> topics = new List<Topic>();

        [Header("Label")]
        [SerializeField] CanvasGroup label;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] float fadeDuration = 0.6f;

        [Header("Pulse")]
        [SerializeField] Transform pulseVisual;
        [SerializeField] float pulseAmount = 0.12f;
        [SerializeField] float pulseSpeed = 1.2f;

        Vector3 pulseBaseScale = Vector3.one;
        Coroutine fade;

        public bool IsOpen { get; private set; }

        void Awake()
        {
            if (pulseVisual != null) pulseBaseScale = pulseVisual.localScale;
            if (label != null) label.alpha = 0f;
        }

        public override void Tap()
        {
            if (!IsInteractable) return;
            if (IsOpen) Hide(); else Show();
            base.Tap();
        }

        public void Show() => Open(title, text);

        public void Show(string topicId)
        {
            Topic topic = topics.Find(t => t != null && t.id == topicId);
            if (topic != null) Open(topic.title, topic.text);
            else Show();
        }

        public void Hide()
        {
            IsOpen = false;
            FadeTo(0f);
        }

        void Open(string heading, string body)
        {
            bool hasHeading = !string.IsNullOrWhiteSpace(heading);
            bool hasBody = !string.IsNullOrWhiteSpace(body);
            if (!hasHeading && !hasBody)
            {
                var experience = GetComponentInParent<MuralExperience>();
                heading = experience != null && experience.Data != null ? experience.Data.DisplayName : null;
                hasHeading = !string.IsNullOrWhiteSpace(heading);
                if (!hasHeading) return;
            }

            if (titleText != null)
            {
                titleText.gameObject.SetActive(hasHeading);
                if (hasHeading) titleText.text = heading.Trim().ToUpperInvariant();
            }
            if (bodyText != null)
            {
                bodyText.gameObject.SetActive(hasBody);
                if (hasBody) bodyText.text = body.Trim();
            }

            IsOpen = true;
            FadeTo(1f);
        }

        void Update()
        {
            if (pulseVisual == null) return;
            float t = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
            pulseVisual.localScale = pulseBaseScale * (1f + pulseAmount * t);
        }

        void LateUpdate()
        {
            if (label == null || label.alpha <= 0f || Camera.main == null) return;
            Transform cam = Camera.main.transform;
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - cam.position, cam.up);
        }

        void FadeTo(float target)
        {
            if (label == null) return;
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Fade(target));
        }

        IEnumerator Fade(float target)
        {
            yield return Tween.Value(label.alpha, target, a => label.alpha = a, fadeDuration, Tween.EaseInOutCubic);
            fade = null;
        }
    }
}
