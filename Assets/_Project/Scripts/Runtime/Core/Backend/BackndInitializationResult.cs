namespace LootUp.Core.Backend
{
    public enum BackndInitializationState
    {
        NotStarted,
        Initializing,
        Initialized,
        Failed
    }

    public readonly struct BackndInitializationResult
    {
        private BackndInitializationResult(
            bool succeeded,
            string message)
        {
            Succeeded = succeeded;
            Message = message ?? string.Empty;
        }

        public bool Succeeded { get; }
        public string Message { get; }

        public static BackndInitializationResult Success()
        {
            return new BackndInitializationResult(true, string.Empty);
        }

        public static BackndInitializationResult Fail(string message)
        {
            return new BackndInitializationResult(false, message);
        }
    }
}
