using System.Collections;
using LootUp.Core.Characters;
using LootUp.Core.SceneFlow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LootUp.Core.Audio
{
    public sealed class GameBgmPlayer : MonoBehaviour
    {
        private const string RuntimeObjectName = "GameBgmPlayer";
        private const string TitleClipPath = "Audio/BGM/LootUp_Title_Loop";
        private const string LobbyClipPath = "Audio/BGM/LootUp_Lobby_Loop";
        private const string InGameNormalClipPath = "Audio/BGM/LootUp_InGame_Normal_Loop";
        private const string InGameFeverClipPath = "Audio/BGM/LootUp_InGame_Fever_Loop";

        private static GameBgmPlayer instance;

        [SerializeField, Range(0f, 1f)]
        private float bgmVolume = 0.4f;

        [SerializeField, Range(0f, 2f)]
        private float crossfadeDurationSeconds = 0.4f;

        private AudioSource primarySource;
        private AudioSource secondarySource;
        private AudioSource activeSource;
        private Coroutine crossfadeCoroutine;
        private Coroutine bindCharacterCoroutine;
        private PlayerCharacterRuntime characterRuntime;
        private AudioClip titleClip;
        private AudioClip lobbyClip;
        private AudioClip inGameNormalClip;
        private AudioClip inGameFeverClip;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateRuntimeInstance()
        {
            if (instance != null)
            {
                return;
            }

            GameObject playerObject = new GameObject(RuntimeObjectName);
            instance = playerObject.AddComponent<GameBgmPlayer>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            ApplySceneBgm(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            StopCharacterBinding();
            instance = null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            StopCharacterBinding();
            ApplySceneBgm(scene);

            if (scene.name == SceneFlowManager.InGameSceneName)
            {
                bindCharacterCoroutine = StartCoroutine(BindCharacterRuntime(scene));
            }
        }

        private void ApplySceneBgm(Scene scene)
        {
            switch (scene.name)
            {
                case SceneFlowManager.TitleSceneName:
                    PlayTrack(ResolveClip(GameBgmId.Title));
                    break;
                case SceneFlowManager.LobbySceneName:
                    PlayTrack(ResolveClip(GameBgmId.Lobby));
                    break;
                case SceneFlowManager.InGameSceneName:
                    PlayTrack(ResolveClip(GameBgmId.InGameNormal));
                    break;
                default:
                    StopAll();
                    break;
            }
        }

        private IEnumerator BindCharacterRuntime(Scene scene)
        {
            const int maxBindFrames = 10;

            for (int frame = 0; frame < maxBindFrames; frame++)
            {
                if (!scene.isLoaded || SceneManager.GetActiveScene() != scene)
                {
                    bindCharacterCoroutine = null;
                    yield break;
                }

                PlayerCharacterRuntime runtime = FindFirstObjectByType<PlayerCharacterRuntime>();
                if (runtime != null && runtime.gameObject.scene == scene)
                {
                    characterRuntime = runtime;
                    characterRuntime.FeverStarted += HandleFeverStarted;
                    characterRuntime.FeverEnded += HandleFeverEnded;

                    if (characterRuntime.IsFeverActive)
                    {
                        PlayTrack(ResolveClip(GameBgmId.InGameFever));
                    }

                    bindCharacterCoroutine = null;
                    yield break;
                }

                yield return null;
            }

            Debug.LogWarning("InGame BGM에 연결할 PlayerCharacterRuntime을 찾지 못했습니다.", this);
            bindCharacterCoroutine = null;
        }

        private void HandleFeverStarted(float durationSeconds)
        {
            PlayTrack(ResolveClip(GameBgmId.InGameFever));
        }

        private void HandleFeverEnded()
        {
            if (SceneManager.GetActiveScene().name == SceneFlowManager.InGameSceneName)
            {
                PlayTrack(ResolveClip(GameBgmId.InGameNormal));
            }
        }

        private void PlayTrack(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            EnsureAudioSources();

            if (activeSource != null && activeSource.isPlaying && activeSource.clip == clip)
            {
                return;
            }

            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
                crossfadeCoroutine = null;
            }

            if (activeSource == null || !activeSource.isPlaying)
            {
                activeSource = primarySource;
                activeSource.clip = clip;
                activeSource.volume = bgmVolume;
                activeSource.Play();
                return;
            }

            AudioSource previousSource = activeSource;
            AudioSource nextSource = activeSource == primarySource ? secondarySource : primarySource;
            nextSource.Stop();
            nextSource.clip = clip;
            nextSource.volume = 0f;
            nextSource.Play();
            activeSource = nextSource;
            crossfadeCoroutine = StartCoroutine(Crossfade(previousSource, nextSource));
        }

        private IEnumerator Crossfade(AudioSource previousSource, AudioSource nextSource)
        {
            float duration = Mathf.Max(0f, crossfadeDurationSeconds);
            float previousStartVolume = previousSource != null ? previousSource.volume : 0f;

            if (duration <= 0f)
            {
                CompleteCrossfade(previousSource, nextSource);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);

                if (previousSource != null)
                {
                    previousSource.volume = Mathf.Lerp(previousStartVolume, 0f, normalized);
                }

                if (nextSource != null)
                {
                    nextSource.volume = Mathf.Lerp(0f, bgmVolume, normalized);
                }

                yield return null;
            }

            CompleteCrossfade(previousSource, nextSource);
        }

        private void CompleteCrossfade(AudioSource previousSource, AudioSource nextSource)
        {
            if (previousSource != null)
            {
                previousSource.Stop();
                previousSource.clip = null;
                previousSource.volume = 0f;
            }

            if (nextSource != null)
            {
                nextSource.volume = bgmVolume;
            }

            crossfadeCoroutine = null;
        }

        private AudioClip ResolveClip(GameBgmId bgmId)
        {
            switch (bgmId)
            {
                case GameBgmId.Title:
                    return titleClip != null ? titleClip : titleClip = Resources.Load<AudioClip>(TitleClipPath);
                case GameBgmId.Lobby:
                    return lobbyClip != null ? lobbyClip : lobbyClip = Resources.Load<AudioClip>(LobbyClipPath);
                case GameBgmId.InGameNormal:
                    return inGameNormalClip != null
                        ? inGameNormalClip
                        : inGameNormalClip = Resources.Load<AudioClip>(InGameNormalClipPath);
                case GameBgmId.InGameFever:
                    return inGameFeverClip != null
                        ? inGameFeverClip
                        : inGameFeverClip = Resources.Load<AudioClip>(InGameFeverClipPath);
                default:
                    return null;
            }
        }

        private void EnsureAudioSources()
        {
            if (primarySource == null)
            {
                primarySource = gameObject.AddComponent<AudioSource>();
            }

            if (secondarySource == null)
            {
                secondarySource = gameObject.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(primarySource);
            ConfigureAudioSource(secondarySource);

            if (activeSource == null)
            {
                activeSource = primarySource;
            }
        }

        private void ConfigureAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
        }

        private void StopAll()
        {
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
                crossfadeCoroutine = null;
            }

            StopSource(primarySource);
            StopSource(secondarySource);
            activeSource = primarySource;
        }

        private void StopCharacterBinding()
        {
            if (bindCharacterCoroutine != null)
            {
                StopCoroutine(bindCharacterCoroutine);
                bindCharacterCoroutine = null;
            }

            if (characterRuntime != null)
            {
                characterRuntime.FeverStarted -= HandleFeverStarted;
                characterRuntime.FeverEnded -= HandleFeverEnded;
                characterRuntime = null;
            }
        }

        private void StopSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.volume = 0f;
        }

        private enum GameBgmId
        {
            Title,
            Lobby,
            InGameNormal,
            InGameFever
        }
    }
}
