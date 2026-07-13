using UnityEngine;
using UnityEngine.SceneManagement;

namespace PH.Core.SceneFlow
{
    public sealed class SceneFlowManager : MonoBehaviour
    {
        public const string LoadingSceneName = "Loading";
        public const string LobbySceneName = "Lobby";
        public const string InGameSceneName = "InGame";

        public static SceneFlowManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad는 루트 GameObject에만 적용되므로 런타임에 Managers를 루트로 분리한다.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        public void LoadLoading()
        {
            LoadScene(LoadingSceneName);
        }

        public void LoadLobby()
        {
            LoadScene(LobbySceneName);
        }

        public void LoadInGame()
        {
            LoadScene(InGameSceneName);
        }

        private void LoadScene(string sceneName)
        {
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                return;
            }

            // PART 1에서는 추가 로딩 연출 없이 기본 씬 전환만 수행한다.
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
