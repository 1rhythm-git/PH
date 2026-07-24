using UnityEngine;

namespace LootUp.Core.Audio
{
    public enum GameSfxId
    {
        Enemy,
        ItemPass,
        ItemGain,
        Logo,
        Damage,
        Walk,
        Run
    }

    public sealed class GameSfxPlayer : MonoBehaviour
    {
        private const string EnemyClipPath = "Audio/SFX/Enemy";
        private const string ItemPassClipPath = "Audio/SFX/pass";
        private const string ItemGainClipPath = "Audio/SFX/gain";
        private const string LogoClipPath = "Audio/SFX/Logo";
        private const string DamageClipPath = "Audio/SFX/Damage";
        private const string WalkClipPath = "Audio/SFX/Walk";
        private const string RunClipPath = "Audio/SFX/Run";

        private static GameSfxPlayer instance;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioSource movementAudioSource;

        [SerializeField, Range(0f, 1f)]
        private float masterVolume = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float movementVolume = 1f;

        private AudioClip enemyClip;
        private AudioClip itemPassClip;
        private AudioClip itemGainClip;
        private AudioClip logoClip;
        private AudioClip damageClip;
        private AudioClip walkClip;
        private AudioClip runClip;

        public static void Play(GameSfxId sfxId)
        {
            GameSfxPlayer player = EnsureInstance();
            if (player == null)
            {
                return;
            }

            player.PlayInternal(sfxId);
        }

        public static void PlayMovement(GameSfxId sfxId)
        {
            if (sfxId != GameSfxId.Walk && sfxId != GameSfxId.Run)
            {
                return;
            }

            GameSfxPlayer player = EnsureInstance();
            player?.PlayMovementInternal(sfxId);
        }

        public static void StopMovement()
        {
            if (instance == null)
            {
                return;
            }

            instance.StopMovementInternal();
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
                instance.EnsureAudioSources();
                return instance;
            }

            GameObject playerObject = new GameObject("GameSfxPlayer");
            DontDestroyOnLoad(playerObject);
            instance = playerObject.AddComponent<GameSfxPlayer>();
            instance.EnsureAudioSources();

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
            EnsureAudioSources();
        }

        private void PlayInternal(GameSfxId sfxId)
        {
            EnsureAudioSources();

            AudioClip clip = ResolveClip(sfxId);
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, masterVolume);
        }

        private void PlayMovementInternal(GameSfxId sfxId)
        {
            EnsureAudioSources();

            AudioClip clip = ResolveClip(sfxId);
            if (movementAudioSource == null || clip == null)
            {
                return;
            }

            // 애니메이션 프레임마다 이전 발소리를 정리하고 현재 프레임 음원을 재생한다.
            movementAudioSource.Stop();
            movementAudioSource.clip = clip;
            movementAudioSource.volume = movementVolume;
            movementAudioSource.Play();
        }

        private void StopMovementInternal()
        {
            if (movementAudioSource == null)
            {
                return;
            }

            movementAudioSource.Stop();
            movementAudioSource.clip = null;
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
                case GameSfxId.Logo:
                    return logoClip != null ? logoClip : logoClip = Resources.Load<AudioClip>(LogoClipPath);
                case GameSfxId.Damage:
                    return damageClip != null ? damageClip : damageClip = Resources.Load<AudioClip>(DamageClipPath);
                case GameSfxId.Walk:
                    return walkClip != null ? walkClip : walkClip = Resources.Load<AudioClip>(WalkClipPath);
                case GameSfxId.Run:
                    return runClip != null ? runClip : runClip = Resources.Load<AudioClip>(RunClipPath);
                default:
                    return null;
            }
        }

        private void EnsureAudioSources()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (movementAudioSource == null || movementAudioSource == audioSource)
            {
                AudioSource[] sources = GetComponents<AudioSource>();
                for (int i = 0; i < sources.Length; i++)
                {
                    if (sources[i] != audioSource)
                    {
                        movementAudioSource = sources[i];
                        break;
                    }
                }
            }

            if (movementAudioSource == null || movementAudioSource == audioSource)
            {
                movementAudioSource = gameObject.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(audioSource);
            ConfigureAudioSource(movementAudioSource);
        }

        private void ConfigureAudioSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
        }
    }
}
