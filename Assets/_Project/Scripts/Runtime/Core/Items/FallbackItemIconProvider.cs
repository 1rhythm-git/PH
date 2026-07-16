using UnityEngine;

namespace PH.Core.Items
{
    public sealed class FallbackItemIconProvider : IItemIconProvider
    {
        private readonly Color defaultScoreColor;
        private Sprite squareSprite;
        private Sprite circleSprite;
        private Sprite heartSprite;

        public FallbackItemIconProvider(Color defaultScoreColor)
        {
            this.defaultScoreColor = defaultScoreColor;
        }

        public ItemIconData GetIcon(ItemDefinition definition)
        {
            ItemType itemType = definition != null ? definition.ItemType : ItemType.Score;
            return new ItemIconData(GetShapeSprite(itemType), GetShapeColor(itemType));
        }

        private Sprite GetShapeSprite(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Time:
                    return GetOrCreateCircleSprite();
                case ItemType.Heal:
                    return GetOrCreateHeartSprite();
                default:
                    return GetOrCreateSquareSprite();
            }
        }

        private Color GetShapeColor(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Time:
                    return new Color(0.15f, 0.78f, 1f, 0.96f);
                case ItemType.Heal:
                    return new Color(1f, 0.08f, 0.24f, 0.96f);
                default:
                    return defaultScoreColor;
            }
        }

        private Sprite GetOrCreateSquareSprite()
        {
            if (squareSprite == null)
            {
                squareSprite = CreateShapeSprite("ItemSquareSprite", IsInsideSquare);
            }

            return squareSprite;
        }

        private Sprite GetOrCreateCircleSprite()
        {
            if (circleSprite == null)
            {
                circleSprite = CreateShapeSprite("ItemCircleSprite", IsInsideCircle);
            }

            return circleSprite;
        }

        private Sprite GetOrCreateHeartSprite()
        {
            if (heartSprite == null)
            {
                heartSprite = CreateShapeSprite("ItemHeartSprite", IsInsideHeart);
            }

            return heartSprite;
        }

        private Sprite CreateShapeSprite(string spriteName, System.Func<float, float, bool> shapePredicate)
        {
            const int textureSize = 64;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.name = spriteName;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float normalizedX = ((float)x / (textureSize - 1)) * 2f - 1f;
                    float normalizedY = ((float)y / (textureSize - 1)) * 2f - 1f;
                    texture.SetPixel(x, y, shapePredicate(normalizedX, normalizedY) ? Color.white : Color.clear);
                }
            }

            texture.Apply(false, true);

            return Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        }

        private static bool IsInsideSquare(float x, float y)
        {
            return Mathf.Abs(x) <= 0.82f && Mathf.Abs(y) <= 0.82f;
        }

        private static bool IsInsideCircle(float x, float y)
        {
            return x * x + y * y <= 0.78f * 0.78f;
        }

        private static bool IsInsideHeart(float x, float y)
        {
            float adjustedY = y + 0.1f;
            float value = x * x + adjustedY * adjustedY - 0.52f;
            return value * value * value - x * x * adjustedY * adjustedY * adjustedY <= 0f;
        }
    }
}
