using LootUp.Core.Audio;
using LootUp.Core.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.Characters
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

        [SerializeField]
        private PlayerMotor playerMotor;

        private float elapsedSeconds;
        private int lastFacingDirection = 1;
        private int lastAnimationFrameIndex = -1;
        private AnimationState lastAnimationState = AnimationState.Idle;
        private bool hasAnimationFrameState;
        private Vector2 baseAnchoredPosition;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Update()
        {
            if (targetImage == null || characterDefinition == null)
            {
                StopMovementSfx();
                return;
            }

            Sprite[] frames = ResolveFrames(out AnimationState animationState);
            if (frames == null || frames.Length == 0)
            {
                targetImage.enabled = false;
                StopMovementSfx();
                return;
            }

            targetImage.enabled = true;
            elapsedSeconds += Time.deltaTime;

            float framesPerSecond = Mathf.Max(1f, characterDefinition.AnimationFramesPerSecond);
            int frameIndex = Mathf.FloorToInt(elapsedSeconds * framesPerSecond) % frames.Length;
            targetImage.sprite = frames[frameIndex];
            ApplyFrameTransform(animationState, frameIndex);
            UpdateMovementSfx(animationState, frameIndex);
        }

        public void Configure(CharacterDefinition definition, PlayerController controller)
        {
            characterDefinition = definition;
            playerController = controller;
            elapsedSeconds = 0f;
            ResetMovementSfxState();
            EnsureReferences();
            baseAnchoredPosition = targetImage != null ? targetImage.rectTransform.anchoredPosition : Vector2.zero;
            ApplyInitialFrame();
        }

        private void OnDisable()
        {
            ResetMovementSfxState();
        }

        private void ApplyInitialFrame()
        {
            if (targetImage == null || characterDefinition == null)
            {
                return;
            }

            Sprite[] frames = ResolveFrames(out AnimationState animationState);
            if (frames == null || frames.Length == 0)
            {
                targetImage.enabled = false;
                return;
            }

            targetImage.enabled = true;
            targetImage.sprite = frames[0];
            ApplyFrameTransform(animationState, 0);
        }

        private void UpdateMovementSfx(AnimationState animationState, int frameIndex)
        {
            if (hasAnimationFrameState
                && lastAnimationState == animationState
                && lastAnimationFrameIndex == frameIndex)
            {
                return;
            }

            hasAnimationFrameState = true;
            lastAnimationState = animationState;
            lastAnimationFrameIndex = frameIndex;

            switch (animationState)
            {
                case AnimationState.Walk:
                    GameSfxPlayer.PlayMovement(GameSfxId.Walk);
                    break;
                case AnimationState.Run:
                    GameSfxPlayer.PlayMovement(GameSfxId.Run);
                    break;
                default:
                    GameSfxPlayer.StopMovement();
                    break;
            }
        }

        private void StopMovementSfx()
        {
            if (!hasAnimationFrameState || lastAnimationState == AnimationState.Idle)
            {
                return;
            }

            ResetMovementSfxState();
        }

        private void ResetMovementSfxState()
        {
            hasAnimationFrameState = false;
            lastAnimationState = AnimationState.Idle;
            lastAnimationFrameIndex = -1;
            GameSfxPlayer.StopMovement();
        }

        private Sprite[] ResolveFrames(out AnimationState animationState)
        {
            bool isMoving = playerController != null
                && playerController.IsMoving
                && (playerMotor == null || !playerMotor.IsMovementLocked);

            // (추가) 이동속도 아이템이 활성화된 동안 기존 Run 스프라이트를 대시 애니메이션으로 사용한다.
            if (isMoving && playerMotor != null && playerMotor.HasActiveMoveSpeedBuff && HasFrames(characterDefinition.RunSprites))
            {
                animationState = AnimationState.Run;
                return characterDefinition.RunSprites;
            }

            if (isMoving && HasFrames(characterDefinition.WalkSprites))
            {
                animationState = AnimationState.Walk;
                return characterDefinition.WalkSprites;
            }

            if (HasFrames(characterDefinition.IdleSprites))
            {
                animationState = AnimationState.Idle;
                return characterDefinition.IdleSprites;
            }

            animationState = AnimationState.Idle;
            return null;
        }

        private void ApplyFrameTransform(AnimationState animationState, int frameIndex)
        {
            if (targetImage == null)
            {
                return;
            }

            if (playerController != null)
            {
                lastFacingDirection = playerController.FacingDirection < 0 ? -1 : 1;
            }

            float frameScale = ResolveFrameScale(animationState, frameIndex);
            RectTransform imageRect = targetImage.rectTransform;
            imageRect.localScale = new Vector3(frameScale * lastFacingDirection, frameScale, 1f);

            // 프레임 배율이 달라도 스프라이트 하단은 기존 바닥 기준선에 고정한다.
            float bottomAlignmentOffset = imageRect.rect.height * (frameScale - 1f) * 0.5f;
            imageRect.anchoredPosition = baseAnchoredPosition + new Vector2(0f, bottomAlignmentOffset);
        }

        private float ResolveFrameScale(AnimationState animationState, int frameIndex)
        {
            switch (animationState)
            {
                case AnimationState.Walk:
                    return characterDefinition.GetWalkFrameScale(frameIndex);
                case AnimationState.Run:
                    return characterDefinition.GetRunFrameScale(frameIndex);
                default:
                    return characterDefinition.GetIdleFrameScale(frameIndex);
            }
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
                baseAnchoredPosition = targetImage.rectTransform.anchoredPosition;
            }

            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
            }

            if (playerMotor == null)
            {
                playerMotor = GetComponentInParent<PlayerMotor>();
            }
        }

        private enum AnimationState
        {
            Idle,
            Walk,
            Run
        }
    }
}
