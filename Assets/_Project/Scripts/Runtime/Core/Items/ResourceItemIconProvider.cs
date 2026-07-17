using UnityEngine;

namespace PH.Core.Items
{
    public sealed class ResourceItemIconProvider : IItemIconProvider
    {
        private readonly ItemIconTable iconTable;
        private readonly IItemIconProvider fallbackProvider;

        public ResourceItemIconProvider(ItemIconTable table, IItemIconProvider fallback)
        {
            iconTable = table;
            fallbackProvider = fallback;
        }

        public ItemIconData GetIcon(ItemDefinition definition)
        {
            if (definition == null || iconTable == null)
            {
                return GetFallbackIcon(definition);
            }

            if (!iconTable.TryGet(definition.IconKey, out ItemIconDefinition iconDefinition))
            {
                return GetFallbackIcon(definition);
            }

            Sprite sprite = LoadSprite(iconDefinition.LocalAddress);
            if (sprite == null && !string.IsNullOrWhiteSpace(iconDefinition.FallbackIconKey) && iconTable.TryGet(iconDefinition.FallbackIconKey, out ItemIconDefinition fallbackDefinition))
            {
                sprite = LoadSprite(fallbackDefinition.LocalAddress);
            }

            return sprite != null ? new ItemIconData(sprite, Color.white) : GetFallbackIcon(definition);
        }

        private ItemIconData GetFallbackIcon(ItemDefinition definition)
        {
            return fallbackProvider != null ? fallbackProvider.GetIcon(definition) : new ItemIconData(null, Color.white);
        }

        private Sprite LoadSprite(string localAddress)
        {
            if (string.IsNullOrWhiteSpace(localAddress))
            {
                return null;
            }

            return Resources.Load<Sprite>(localAddress);
        }
    }
}
