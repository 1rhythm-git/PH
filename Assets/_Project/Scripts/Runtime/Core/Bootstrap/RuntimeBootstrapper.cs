using System.Collections;
using PH.Core.SceneFlow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace PH.Core.Bootstrap
{
    public sealed class RuntimeBootstrapper : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("loadLobbyOnStart")]
        private bool loadTitleOnStart = true;

        [SerializeField]
        private float loadingDuration = 1.5f;

        [SerializeField]
        private float completedHoldDuration = 0.4f;

        [SerializeField]
        private GameObject logoAHighlight;

        private IEnumerator Start()
        {
            if (!loadTitleOnStart)
            {
                yield break;
            }

            SetLogoHighlightVisible(false);

            float elapsedTime = 0f;

            while (elapsedTime < loadingDuration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetLogoHighlightVisible(true);

            if (completedHoldDuration > 0f)
            {
                // LAF 로고 A 하이라이트를 잠시 보여준 뒤 Title로 이동한다.
                yield return new WaitForSeconds(completedHoldDuration);
            }

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadTitle();
                yield break;
            }

            SceneManager.LoadScene(SceneFlowManager.TitleSceneName, LoadSceneMode.Single);
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
