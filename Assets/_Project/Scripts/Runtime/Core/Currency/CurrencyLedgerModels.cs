using System;
using LootUp.Core.Profile;

namespace LootUp.Core.Currency
{
    public enum CurrencyLedgerOperationState
    {
        Applied,
        Duplicate,
        Queued,
        InsufficientFunds,
        InvalidRequest,
        Error
    }

    public readonly struct CurrencyBalanceSnapshot
    {
        public CurrencyBalanceSnapshot(int gameMoney, int ruby)
        {
            GameMoney = Math.Max(0, gameMoney);
            Ruby = Math.Max(0, ruby);
        }

        public int GameMoney { get; }
        public int Ruby { get; }

        public int GetAmount(UserCurrencyType currencyType)
        {
            return currencyType == UserCurrencyType.Ruby
                ? Ruby
                : GameMoney;
        }
    }

    public readonly struct CurrencyLedgerRequest
    {
        public CurrencyLedgerRequest(
            string requestId,
            UserCurrencyType currencyType,
            int deltaAmount,
            string reason,
            string runId,
            string createdAt)
        {
            RequestId = requestId?.Trim() ?? string.Empty;
            CurrencyType = currencyType;
            DeltaAmount = deltaAmount;
            Reason = reason?.Trim() ?? string.Empty;
            RunId = runId?.Trim() ?? string.Empty;
            CreatedAt = createdAt?.Trim() ?? string.Empty;
        }

        public string RequestId { get; }
        public UserCurrencyType CurrencyType { get; }
        public int DeltaAmount { get; }
        public string Reason { get; }
        public string RunId { get; }
        public string CreatedAt { get; }
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(RequestId)
            && DeltaAmount != 0;
    }

    public readonly struct CurrencyLedgerOperationResult
    {
        public CurrencyLedgerOperationResult(
            CurrencyLedgerOperationState state,
            CurrencyBalanceSnapshot balances,
            bool hasAuthoritativeBalances,
            string message)
        {
            State = state;
            Balances = balances;
            HasAuthoritativeBalances = hasAuthoritativeBalances;
            Message = message ?? string.Empty;
        }

        public CurrencyLedgerOperationState State { get; }
        public CurrencyBalanceSnapshot Balances { get; }
        public bool HasAuthoritativeBalances { get; }
        public string Message { get; }
        public bool Completed =>
            State == CurrencyLedgerOperationState.Applied
            || State == CurrencyLedgerOperationState.Duplicate;
    }

    public readonly struct CurrencySynchronizationResult
    {
        public CurrencySynchronizationResult(
            bool succeeded,
            bool migrated,
            CurrencyBalanceSnapshot balances,
            string message)
        {
            Succeeded = succeeded;
            Migrated = migrated;
            Balances = balances;
            Message = message ?? string.Empty;
        }

        public bool Succeeded { get; }
        public bool Migrated { get; }
        public CurrencyBalanceSnapshot Balances { get; }
        public string Message { get; }
    }
}
