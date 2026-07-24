namespace LootUp.Core.Items
{
    public sealed class AddCollectionItemEffect : IItemEffect
    {
        public bool CanExecute(ItemDefinition definition)
        {
            return definition != null && definition.ItemType == ItemType.Collection;
        }

        public ItemEffectResult Execute(ItemDefinition definition, ItemEffectContext context)
        {
            if (definition == null)
            {
                return ItemEffectResult.None;
            }

            if (context.RunItemEventRecorder != null && context.RunItemEventRecorder.HasReachedAcquireLimit(definition))
            {
                context.TopHUDController?.SetItemStatus("RUN LIMIT");
                return new ItemEffectResult(ItemEffectOutcome.CollectionRunLimitReached, 0, context.EventId, CollectionChangeStatus.RunLimitReached);
            }

            CollectionChangeResult result = ItemCollectionManager.TryAcquire(definition, context.EventId);
            switch (result.Status)
            {
                case CollectionChangeStatus.Added:
                case CollectionChangeStatus.SavePending:
                    context.TopHUDController?.SetItemStatus($"{definition.DisplayName} +{result.AddedAmount}");
                    return new ItemEffectResult(ItemEffectOutcome.CollectionAdded, result.AddedAmount, result.EventId, result.Status);
                case CollectionChangeStatus.AlreadyOwned:
                    context.TopHUDController?.SetItemStatus("ALREADY OWNED");
                    return new ItemEffectResult(ItemEffectOutcome.CollectionAlreadyOwned, 0, result.EventId, result.Status);
                case CollectionChangeStatus.OwnedLimitReached:
                    context.TopHUDController?.SetItemStatus("OWNED LIMIT");
                    return new ItemEffectResult(ItemEffectOutcome.CollectionOwnedLimitReached, 0, result.EventId, result.Status);
                case CollectionChangeStatus.DuplicateEvent:
                    context.TopHUDController?.SetItemStatus("ALREADY PROCESSED");
                    return new ItemEffectResult(ItemEffectOutcome.CollectionDuplicateEvent, 0, result.EventId, result.Status);
                default:
                    context.TopHUDController?.SetItemStatus(definition.DisplayName);
                    return new ItemEffectResult(ItemEffectOutcome.None, 0, result.EventId, result.Status);
            }
        }
    }
}
