namespace PH.Core.Items
{
    public enum ItemEffectOutcome
    {
        None = 0,
        ScoreAdded = 1,
        TimeAdded = 2,
        LifeHealed = 3,
        MaxLifeIncreased = 4,
        MoveSpeedIncreased = 5,
        CollectionAdded = 6,
        CollectionAlreadyOwned = 7,
        CollectionOwnedLimitReached = 8,
        CollectionRunLimitReached = 9,
        CollectionDuplicateEvent = 10
    }

    public readonly struct ItemEffectResult
    {
        public static readonly ItemEffectResult None = new ItemEffectResult(ItemEffectOutcome.None, 0);

        public ItemEffectResult(ItemEffectOutcome outcome, int value)
            : this(outcome, value, string.Empty, CollectionChangeStatus.InvalidDefinition)
        {
        }

        public ItemEffectResult(ItemEffectOutcome outcome, int value, string eventId, CollectionChangeStatus collectionStatus)
        {
            Outcome = outcome;
            Value = value;
            EventId = eventId ?? string.Empty;
            CollectionStatus = collectionStatus;
        }

        public ItemEffectOutcome Outcome { get; }
        public int Value { get; }
        public string EventId { get; }
        public CollectionChangeStatus CollectionStatus { get; }
        public bool Applied => Outcome != ItemEffectOutcome.None
            && Outcome != ItemEffectOutcome.CollectionAlreadyOwned
            && Outcome != ItemEffectOutcome.CollectionOwnedLimitReached
            && Outcome != ItemEffectOutcome.CollectionRunLimitReached
            && Outcome != ItemEffectOutcome.CollectionDuplicateEvent;
    }
}
