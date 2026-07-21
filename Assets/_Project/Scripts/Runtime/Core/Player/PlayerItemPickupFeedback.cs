using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Player
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PlayerItemPickupFeedback : MonoBehaviour
    {
        [SerializeField]
        private Vector2 startOffset = new Vector2(0f, 64f);

        [SerializeField]
        private float riseDistance = 34f;

        [SerializeField]
        private float durationSeconds = 0.72f;

        [SerializeField]
        private int fontSize = 28;

        [SerializeField]
        private Vector2 skillStackOffset = new Vector2(0f, 52f);

        [SerializeField]
        private float skillSizeMultiplier = 1.35f;

        [SerializeField]
        private Color skillColor = new Color(1f, 0.86f, 0.18f, 1f);

        [SerializeField]
        private Color outlineColor = new Color(0f, 0f, 0f, 0.82f);

        [SerializeField]
        private Color shadowColor = new Color(0f, 0f, 0f, 0.55f);

        public void Show(string message, Color color)
        {
            Show(message, color, 1f, Vector2.zero);
        }

        public void Show(string message, Color color, float sizeMultiplier)
        {
            Show(message, color, sizeMultiplier, Vector2.zero);
        }

        public void ShowSkillActivation(bool stackAboveItemFeedback)
        {
            Vector2 additionalOffset = stackAboveItemFeedback ? skillStackOffset : Vector2.zero;
            Show("SKILL ON!", skillColor, skillSizeMultiplier, additionalOffset);
        }

        private void Show(string message, Color color, float sizeMultiplier, Vector2 additionalOffset)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            GameObject textObject = new GameObject("ItemPickupFeedbackText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline), typeof(Shadow));
            textObject.layer = gameObject.layer;
            textObject.transform.SetParent(transform, false);
            textObject.transform.SetAsLastSibling();

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            Vector2 animationStartOffset = startOffset + additionalOffset;
            textRect.anchoredPosition = animationStartOffset;
            textRect.sizeDelta = new Vector2(220f, 44f);
            textRect.localScale = Vector3.one;

            Text feedbackText = textObject.GetComponent<Text>();
            feedbackText.text = message;
            feedbackText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            feedbackText.fontSize = Mathf.Max(1, Mathf.RoundToInt(fontSize * Mathf.Max(0.1f, sizeMultiplier)));
            feedbackText.fontStyle = FontStyle.Bold;
            feedbackText.alignment = TextAnchor.MiddleCenter;
            feedbackText.horizontalOverflow = HorizontalWrapMode.Overflow;
            feedbackText.verticalOverflow = VerticalWrapMode.Overflow;
            feedbackText.color = color;
            feedbackText.raycastTarget = false;

            Outline outline = textObject.GetComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            Shadow[] shadowComponents = textObject.GetComponents<Shadow>();
            Shadow shadow = shadowComponents[shadowComponents.Length - 1];
            shadow.effectColor = shadowColor;
            shadow.effectDistance = new Vector2(0f, -3f);
            shadow.useGraphicAlpha = true;

            StartCoroutine(AnimateAndDestroy(textObject, textRect, feedbackText, animationStartOffset));
        }

        private IEnumerator AnimateAndDestroy(GameObject textObject, RectTransform textRect, Text feedbackText, Vector2 animationStartOffset)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, durationSeconds);
            Vector2 from = animationStartOffset;
            Vector2 to = animationStartOffset + new Vector2(0f, Mathf.Max(0f, riseDistance));
            Color startColor = feedbackText.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 2f);

                if (textRect != null)
                {
                    textRect.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                }

                if (feedbackText != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.35f) / 0.65f));
                    feedbackText.color = color;
                }

                yield return null;
            }

            if (textObject != null)
            {
                Destroy(textObject);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            durationSeconds = Mathf.Max(0.01f, durationSeconds);
            riseDistance = Mathf.Max(0f, riseDistance);
            fontSize = Mathf.Max(1, fontSize);
            skillSizeMultiplier = Mathf.Max(0.1f, skillSizeMultiplier);
        }
#endif
    }
}
