using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(PlayerController), typeof(PlayerMotor))]
    public sealed class PlayerMovementDustFeedback : MonoBehaviour
    {
        [Header("Normal Move Dust")]
        [SerializeField]
        private float normalEmissionInterval = 0.12f;

        [SerializeField]
        private int normalParticlesPerBurst = 2;

        [SerializeField]
        private Color normalDustColor = new Color(0.76f, 0.72f, 0.64f, 0.68f);

        [Header("Dash Dust")]
        [SerializeField]
        private float dashEmissionInterval = 0.05f;

        [SerializeField]
        private int dashParticlesPerBurst = 4;

        [SerializeField]
        private Color dashDustColor = new Color(0.5f, 0.9f, 1f, 0.88f);

        [Header("Pool")]
        [SerializeField]
        private int maxParticleCount = 36;

        private readonly List<DustParticle> particles = new List<DustParticle>();
        private RectTransform playerRect;
        private PlayerController playerController;
        private PlayerMotor playerMotor;
        private float nextEmissionTime;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            nextEmissionTime = Time.time;
        }

        private void Update()
        {
            UpdateParticles(Time.deltaTime);
            EnsureReferences();

            if (playerController == null || playerMotor == null || !playerController.IsMoving || playerMotor.IsMovementLocked)
            {
                return;
            }

            bool isDashing = playerMotor.HasActiveMoveSpeedBuff;
            if (Time.time < nextEmissionTime)
            {
                return;
            }

            SpawnBurst(playerController.MoveDirection, isDashing);
            float interval = isDashing ? dashEmissionInterval : normalEmissionInterval;
            nextEmissionTime = Time.time + Mathf.Max(0.01f, interval);
        }

        private void OnDisable()
        {
            DeactivateAllParticles();
        }

        private void OnDestroy()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                if (particles[i].RectTransform != null)
                {
                    Destroy(particles[i].RectTransform.gameObject);
                }
            }

            particles.Clear();
        }

        private void SpawnBurst(int moveDirection, bool isDashing)
        {
            if (playerRect == null || playerRect.parent == null)
            {
                return;
            }

            int direction = moveDirection < 0 ? -1 : 1;
            int particleCount = isDashing ? dashParticlesPerBurst : normalParticlesPerBurst;
            particleCount = Mathf.Max(1, particleCount);

            // (추가) 진행 방향 반대쪽 발밑을 먼지 발생 기준점으로 사용한다.
            Vector2 playerPosition = playerRect.anchoredPosition;
            float footOffsetX = playerRect.rect.width * 0.24f;
            float footOffsetY = playerRect.rect.height * 0.46f;
            Vector2 emissionPosition = playerPosition + new Vector2(-direction * footOffsetX, -footOffsetY);

            for (int i = 0; i < particleCount; i++)
            {
                SpawnParticle(emissionPosition, direction, isDashing, i);
            }
        }

        private void SpawnParticle(Vector2 emissionPosition, int moveDirection, bool isDashing, int particleIndex)
        {
            DustParticle particle = GetParticle();
            if (particle == null)
            {
                return;
            }

            float horizontalJitter = Random.Range(-4f, 4f);
            float verticalJitter = Random.Range(-2f, 3f);
            particle.RectTransform.anchoredPosition = emissionPosition + new Vector2(horizontalJitter, verticalJitter);

            if (isDashing && particleIndex % 2 == 0)
            {
                particle.RectTransform.sizeDelta = new Vector2(Random.Range(15f, 25f), Random.Range(3f, 6f));
            }
            else
            {
                float size = isDashing ? Random.Range(7f, 13f) : Random.Range(4f, 8f);
                particle.RectTransform.sizeDelta = new Vector2(size * Random.Range(1f, 1.5f), size);
            }

            particle.RectTransform.localScale = Vector3.one;
            particle.RectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));
            // (추가) 풀에서 재사용한 입자도 항상 플레이어 바로 뒤에서 렌더링한다.
            particle.RectTransform.SetSiblingIndex(Mathf.Max(0, transform.GetSiblingIndex() - 1));

            float horizontalSpeed = isDashing ? Random.Range(50f, 92f) : Random.Range(18f, 42f);
            float verticalSpeed = isDashing ? Random.Range(18f, 40f) : Random.Range(10f, 27f);
            particle.Velocity = new Vector2(-moveDirection * horizontalSpeed, verticalSpeed);
            particle.Lifetime = isDashing ? Random.Range(0.34f, 0.52f) : Random.Range(0.24f, 0.36f);
            particle.Age = 0f;
            particle.BaseColor = isDashing ? dashDustColor : normalDustColor;
            particle.Image.color = particle.BaseColor;
            particle.RectTransform.gameObject.SetActive(true);
        }

        private DustParticle GetParticle()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].RectTransform != null && !particles[i].RectTransform.gameObject.activeSelf)
                {
                    return particles[i];
                }
            }

            if (particles.Count >= Mathf.Max(1, maxParticleCount))
            {
                return FindOldestParticle();
            }

            return CreateParticle();
        }

        private DustParticle CreateParticle()
        {
            if (playerRect == null || playerRect.parent == null)
            {
                return null;
            }

            GameObject particleObject = new GameObject("MovementDust", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            particleObject.layer = gameObject.layer;
            particleObject.transform.SetParent(playerRect.parent, false);

            RectTransform particleRect = particleObject.GetComponent<RectTransform>();
            particleRect.anchorMin = new Vector2(0.5f, 0.5f);
            particleRect.anchorMax = new Vector2(0.5f, 0.5f);
            particleRect.pivot = new Vector2(0.5f, 0.5f);

            Image particleImage = particleObject.GetComponent<Image>();
            particleImage.raycastTarget = false;

            DustParticle particle = new DustParticle(particleRect, particleImage);
            particles.Add(particle);
            particleObject.SetActive(false);
            return particle;
        }

        private DustParticle FindOldestParticle()
        {
            DustParticle oldestParticle = null;
            float highestNormalizedAge = -1f;

            for (int i = 0; i < particles.Count; i++)
            {
                DustParticle particle = particles[i];
                float normalizedAge = particle.Lifetime > 0f ? particle.Age / particle.Lifetime : 1f;
                if (normalizedAge > highestNormalizedAge)
                {
                    oldestParticle = particle;
                    highestNormalizedAge = normalizedAge;
                }
            }

            return oldestParticle;
        }

        private void UpdateParticles(float deltaTime)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                DustParticle particle = particles[i];
                if (particle.RectTransform == null || !particle.RectTransform.gameObject.activeSelf)
                {
                    continue;
                }

                particle.Age += Mathf.Max(0f, deltaTime);
                if (particle.Age >= particle.Lifetime)
                {
                    particle.RectTransform.gameObject.SetActive(false);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(particle.Age / Mathf.Max(0.01f, particle.Lifetime));
                particle.RectTransform.anchoredPosition += particle.Velocity * deltaTime;
                particle.Velocity += Vector2.down * (55f * deltaTime);
                particle.RectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.45f, normalizedAge);

                float alpha = particle.BaseColor.a * (1f - normalizedAge) * (1f - normalizedAge);
                particle.Image.color = new Color(particle.BaseColor.r, particle.BaseColor.g, particle.BaseColor.b, alpha);
            }
        }

        private void DeactivateAllParticles()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].RectTransform != null)
                {
                    particles[i].RectTransform.gameObject.SetActive(false);
                }
            }
        }

        private void EnsureReferences()
        {
            if (playerRect == null)
            {
                playerRect = GetComponent<RectTransform>();
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (playerMotor == null)
            {
                playerMotor = GetComponent<PlayerMotor>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            normalEmissionInterval = Mathf.Max(0.01f, normalEmissionInterval);
            dashEmissionInterval = Mathf.Max(0.01f, dashEmissionInterval);
            normalParticlesPerBurst = Mathf.Max(1, normalParticlesPerBurst);
            dashParticlesPerBurst = Mathf.Max(1, dashParticlesPerBurst);
            maxParticleCount = Mathf.Max(1, maxParticleCount);
        }
#endif

        private sealed class DustParticle
        {
            public DustParticle(RectTransform rectTransform, Image image)
            {
                RectTransform = rectTransform;
                Image = image;
            }

            public RectTransform RectTransform { get; }
            public Image Image { get; }
            public Vector2 Velocity { get; set; }
            public Color BaseColor { get; set; }
            public float Age { get; set; }
            public float Lifetime { get; set; }
        }
    }
}
