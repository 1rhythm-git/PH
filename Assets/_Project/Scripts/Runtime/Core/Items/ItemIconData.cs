using UnityEngine;

namespace PH.Core.Items
{
    public readonly struct ItemIconData
    {
        public ItemIconData(Sprite sprite, Color color)
        {
            Sprite = sprite;
            Color = color;
        }

        public Sprite Sprite { get; }
        public Color Color { get; }
    }
}
