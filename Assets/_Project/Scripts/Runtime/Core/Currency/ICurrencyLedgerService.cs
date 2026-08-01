using System.Threading.Tasks;

namespace LootUp.Core.Currency
{
    public interface ICurrencyLedgerService
    {
        bool IsOnline { get; }
        Task<CurrencySynchronizationResult> SynchronizeAsync(
            CurrencyBalanceSnapshot localBalances);
        Task<CurrencyLedgerOperationResult> ApplyAsync(
            CurrencyLedgerRequest request);
    }
}
