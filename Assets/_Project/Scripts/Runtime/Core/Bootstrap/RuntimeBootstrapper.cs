using System.Collections;
using PH.Core.SceneFlow;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Bootstrap
{
    public sealed class RuntimeBootstrapper : MonoBehaviour
    {
        [SerializeField]
        private bool loadLobbyOnStart = true;

        [SerializeField]
        private Image loadingBarFill;

        [SerializeField]
        private float loadingDuration = 3f;

        [SerializeField]
        private float completedHoldDuration = 0.4f;

        [SerializeField]
        private GameObject logoAHighlight;

        private RectTransform loadingBarFillRect;

        private IEnumerator Start()
        {
            if (!loadLobbyOnStart)
            {
                yield break;
            }

            PrepareLoadingBar();
            SetLogoHighlightVisible(false);
            SetLoadingProgress(0f);

            float elapsedTime = 0f;

            while (elapsedTime < loadingDuration)
            {
                elapsedTime += Time.deltaTime;
                SetLoadingProgress(Mathf.Clamp01(elapsedTime / loadingDuration));

                yield return null;
            }

            SetLoadingProgress(1f);
            SetLogoHighlightVisible(true);

            if (completedHoldDuration > 0f)
            {
                // 100% 상태와 LAF 로고 A 하이라이트를 잠시 보여준 뒤 Lobby로 이동한다.
                yield return new WaitForSeconds(completedHoldDuration);
            }

            SceneFlowManager.Instance.LoadLobby();
        }

        private void PrepareLoadingBar()
        {
            if (loadingBarFill == null)
            {
                return;
            }

            loadingBarFillRect = loadingBarFill.rectTransform;

            // 색상은 건드리지 않고, RectTransform 폭만 조절해 게이지처럼 채운다.
            loadingBarFill.type = Image.Type.Simple;
            loadingBarFillRect.anchorMin = new Vector2(0f, 0f);
            loadingBarFillRect.anchorMax = new Vector2(0f, 1f);
            loadingBarFillRect.pivot = new Vector2(0f, 0.5f);
            loadingBarFillRect.offsetMin = new Vector2(4f, 4f);
            loadingBarFillRect.offsetMax = new Vector2(-4f, -4f);
        }

        private void SetLoadingProgress(float progress)
        {
            if (loadingBarFill == null)
            {
                return;
            }

            if (loadingBarFillRect == null)
            {
                loadingBarFillRect = loadingBarFill.rectTransform;
            }

            loadingBarFill.fillAmount = progress;
            loadingBarFillRect.anchorMax = new Vector2(progress, 1f);
        }

        private void SetLogoHighlightVisible(bool isVisible)
        {
            if (logoAHighlight == null)
            {
                return;
            }

            logoAHighlight.SetActive(isVisible);
        }
    }
}
