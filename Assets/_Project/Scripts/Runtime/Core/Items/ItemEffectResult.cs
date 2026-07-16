namespace PH.Core.Items
{
    public enum ItemEffectOutcome
    {
        None = 0,
        ScoreAdded = 1,
        TimeAdded = 2,
        LifeHealed = 3,
        MaxLifeIncreased = 4,
        MoveSpeedIncreased = 5
    }

    public readonly struct ItemEffectResult
    {
        public static readonly ItemEffectResult None = new ItemEffectResult(ItemEffectOutcome.None, 0);

        public ItemEffectResult(ItemEffectOutcome outcome, int value)
        {
            Outcome = outcome;
            Value = value;
        }

        public ItemEffectOutcome Outcome { get; }
        public int Value { get; }
        public bool Applied => Outcome != ItemEffectOutcome.None;
    }
}
