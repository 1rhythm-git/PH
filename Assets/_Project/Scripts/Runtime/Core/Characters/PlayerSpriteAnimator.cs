using PH.Core.Player;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Characters
{
    [RequireComponent(typeof(Image))]
    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        [SerializeField]
        private Image targetImage;

        [SerializeField]
        private CharacterDefinition characterDefinition;

        [SerializeField]
        private PlayerController playerController;

        private float elapsedSeconds;
        private int lastFacingDirection = 1;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Update()
        {
            if (targetImage == null || characterDefinition == null)
            {
                return;
            }

            Sprite[] frames = ResolveFrames();
            if (frames == null || frames.Length == 0)
            {
                targetImage.enabled = false;
                return;
            }

            targetImage.enabled = true;
            elapsedSeconds += Time.deltaTime;

            float framesPerSecond = Mathf.Max(1f, characterDefinition.AnimationFramesPerSecond);
            int frameIndex = Mathf.FloorToInt(elapsedSeconds * framesPerSecond) % frames.Length;
            targetImage.sprite = frames[frameIndex];
            ApplyFacing();
        }

        public void Configure(CharacterDefinition definition, PlayerController controller)
        {
            characterDefinition = definition;
            playerController = controller;
            elapsedSeconds = 0f;
            EnsureReferences();
            ApplyInitialFrame();
        }

        private void ApplyInitialFrame()
        {
            if (targetImage == null || characterDefinition == null)
            {
                return;
            }

            Sprite[] frames = ResolveFrames();
            if (frames == null || frames.Length == 0)
            {
                targetImage.enabled = false;
                return;
            }

            targetImage.enabled = true;
            targetImage.sprite = frames[0];
            ApplyFacing();
        }

        private Sprite[] ResolveFrames()
        {
            bool isMoving = playerController != null && playerController.IsMoving;

            if (isMoving && HasFrames(characterDefinition.WalkSprites))
            {
                return characterDefinition.WalkSprites;
            }

            if (HasFrames(characterDefinition.IdleSprites))
            {
                return characterDefinition.IdleSprites;
            }

            return null;
        }

        private void ApplyFacing()
        {
            if (targetImage == null)
            {
                return;
            }

            if (playerController != null)
            {
                lastFacingDirection = playerController.FacingDirection < 0 ? -1 : 1;
            }

            RectTransform imageRect = targetImage.rectTransform;
            Vector3 scale = imageRect.localScale;
            scale.x = Mathf.Abs(scale.x) * lastFacingDirection;
            imageRect.localScale = scale;
        }

        private bool HasFrames(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureReferences()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }
    }
}
