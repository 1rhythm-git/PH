using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Player
{
    public sealed class PlayerBuffVisualFeedback : MonoBehaviour
    {
        [SerializeField]
        private float blinkIntervalSeconds = 0.12f;

        [SerializeField]
        private float dimmedAlpha = 0.42f;

        private readonly List<Graphic> graphics = new List<Graphic>();
        private readonly Dictionary<Graphic, Color> originalColors = new Dictionary<Graphic, Color>();
        private float blinkEndTime;
        private float nextBlinkTime;
        private bool isBlinking;
        private bool isDimmed;

        private void Update()
        {
            if (!isBlinking)
            {
                return;
            }

            if (Time.time >= blinkEndTime)
            {
                StopBlink();
                return;
            }

            if (Time.time < nextBlinkTime)
            {
                return;
            }

            isDimmed = !isDimmed;
            ApplyAlpha(isDimmed ? dimmedAlpha : 1f);
            nextBlinkTime = Time.time + Mathf.Max(0.01f, blinkIntervalSeconds);
        }

        private void OnDisable()
        {
            StopBlink();
        }

        public void PlayBlink(float durationSeconds)
        {
            float clampedDuration = Mathf.Max(0f, durationSeconds);
            if (clampedDuration <= 0f)
            {
                return;
            }

            RefreshGraphics();
            // (변경) 실제 이동속도 버프와 동일하게 새 아이템의 지속시간으로 점멸 종료 시각을 갱신한다.
            blinkEndTime = Time.time + clampedDuration;
            nextBlinkTime = Time.time;
            isBlinking = true;
        }

        private void StopBlink()
        {
            if (!isBlinking && originalColors.Count == 0)
            {
                return;
            }

            RestoreOriginalColors();
            isBlinking = false;
            isDimmed = false;
            blinkEndTime = 0f;
            nextBlinkTime = 0f;
        }

        private void RefreshGraphics()
        {
            graphics.Clear();
            GetComponentsInChildren<Graphic>(true, graphics);

            for (int i = 0; i < graphics.Count; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic != null && !originalColors.ContainsKey(graphic))
                {
                    originalColors.Add(graphic, graphic.color);
                }
            }
        }

        private void ApplyAlpha(float alpha)
        {
            for (int i = graphics.Count - 1; i >= 0; i--)
            {
                Graphic graphic = graphics[i];
                if (graphic == null)
                {
                    graphics.RemoveAt(i);
                    continue;
                }

                if (!originalColors.TryGetValue(graphic, out Color originalColor))
                {
                    originalColor = graphic.color;
                    originalColors.Add(graphic, originalColor);
                }

                graphic.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * Mathf.Clamp01(alpha));
            }
        }

        private void RestoreOriginalColors()
        {
            foreach (KeyValuePair<Graphic, Color> pair in originalColors)
            {
                if (pair.Key != null)
                {
                    pair.Key.color = pair.Value;
                }
            }

            originalColors.Clear();
            graphics.Clear();
        }
    }
}
