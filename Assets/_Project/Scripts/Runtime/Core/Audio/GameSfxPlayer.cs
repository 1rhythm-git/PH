using UnityEngine;

namespace PH.Core.Audio
{
    public enum GameSfxId
    {
        Enemy,
        ItemPass,
        ItemGain
    }

    public sealed class GameSfxPlayer : MonoBehaviour
    {
        private const string EnemyClipPath = "Audio/SFX/Enemy";
        private const string ItemPassClipPath = "Audio/SFX/pass";
        private const string ItemGainClipPath = "Audio/SFX/gain";

        private static GameSfxPlayer instance;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField, Range(0f, 1f)]
        private float masterVolume = 0.5f;

        private AudioClip enemyClip;
        private AudioClip itemPassClip;
        private AudioClip itemGainClip;

        public static void Play(GameSfxId sfxId)
        {
            GameSfxPlayer player = EnsureInstance();
            if (player == null)
            {
                return;
            }

            player.PlayInternal(sfxId);
        }

        private static GameSfxPlayer EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindFirstObjectByType<GameSfxPlayer>();
            if (instance != null)
            {
                instance.EnsureAudioSource();
                return instance;
            }

            GameObject playerObject = new GameObject("GameSfxPlayer");
            DontDestroyOnLoad(playerObject);
            instance = playerObject.AddComponent<GameSfxPlayer>();
            instance.EnsureAudioSource();

            return instance;
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
            EnsureAudioSource();
        }

        private void PlayInternal(GameSfxId sfxId)
        {
            EnsureAudioSource();

            AudioClip clip = ResolveClip(sfxId);
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, masterVolume);
        }

        private AudioClip ResolveClip(GameSfxId sfxId)
        {
            switch (sfxId)
            {
                case GameSfxId.Enemy:
                    return enemyClip != null ? enemyClip : enemyClip = Resources.Load<AudioClip>(EnemyClipPath);
                case GameSfxId.ItemPass:
                    return itemPassClip != null ? itemPassClip : itemPassClip = Resources.Load<AudioClip>(ItemPassClipPath);
                case GameSfxId.ItemGain:
                    return itemGainClip != null ? itemGainClip : itemGainClip = Resources.Load<AudioClip>(ItemGainClipPath);
                default:
                    return null;
            }
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }
}
