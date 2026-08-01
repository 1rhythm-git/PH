using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using LootUp.Core.Profile;
using UnityEngine;

namespace LootUp.Core.Currency
{
    public static class CurrencyLedgerManager
    {
        private static readonly SemaphoreSlim OperationGate =
            new SemaphoreSlim(1, 1);

        private static ICurrencyLedgerService service;
        private static PendingCurrencyTransactionStore pendingStore;
        private static int configurationVersion;

        public static bool UsesServerAuthority => service != null;

        public static void Configure(
            ICurrencyLedgerService currencyLedgerService,
            string userId)
        {
            Interlocked.Increment(ref configurationVersion);
            service = currencyLedgerService;
            pendingStore = service != null
                ? new PendingCurrencyTransactionStore(userId)
                : null;
        }

        public static async Task<CurrencySynchronizationResult>
            InitializeAsync()
        {
            CurrencyBalanceSnapshot localBalances = GetLocalBalances();
            ICurrencyLedgerService activeService = service;
            PendingCurrencyTransactionStore activeStore = pendingStore;
            int activeVersion = configurationVersion;
            if (activeService == null)
            {
                return new CurrencySynchronizationResult(
                    true,
                    false,
                    localBalances,
                    string.Empty);
            }

            await OperationGate.WaitAsync();
            try
            {
                if (activeVersion != configurationVersion)
                {
                    return CreateSynchronizationChangedResult(localBalances);
                }

                if (!activeService.IsOnline)
                {
                    return new CurrencySynchronizationResult(
                        false,
                        false,
                        localBalances,
                        "Currency server is offline.");
                }

                CurrencySynchronizationResult synchronization =
                    await activeService.SynchronizeAsync(localBalances);
                if (!synchronization.Succeeded)
                {
                    Debug.LogWarning(
                        $"Currency synchronization failed: {synchronization.Message}");
                    return synchronization;
                }

                if (activeVersion != configurationVersion)
                {
                    return CreateSynchronizationChangedResult(localBalances);
                }

                ApplyAuthoritativeBalances(synchronization.Balances);
                await FlushPendingRequestsAsync(
                    activeService,
                    activeStore,
                    activeVersion);
                return new CurrencySynchronizationResult(
                    true,
                    synchronization.Migrated,
                    GetLocalBalances(),
                    synchronization.Message);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Currency synchronization failed: {exception.Message}");
                return new CurrencySynchronizationResult(
                    false,
                    false,
                    GetLocalBalances(),
                    exception.Message);
            }
            finally
            {
                OperationGate.Release();
            }
        }

        public static Task<CurrencyLedgerOperationResult> AddCurrencyAsync(
            UserCurrencyType currencyType,
            int amount,
            string requestId,
            string reason,
            string runId = "")
        {
            if (amount <= 0)
            {
                return Task.FromResult(CreateInvalidResult());
            }

            return ApplyDeltaAsync(
                currencyType,
                amount,
                requestId,
                reason,
                runId);
        }

        public static Task<CurrencyLedgerOperationResult> SpendCurrencyAsync(
            UserCurrencyType currencyType,
            int amount,
            string requestId,
            string reason,
            string runId = "")
        {
            if (amount <= 0)
            {
                return Task.FromResult(CreateInvalidResult());
            }

            return ApplyDeltaAsync(
                currencyType,
                -amount,
                requestId,
                reason,
                runId);
        }

        private static async Task<CurrencyLedgerOperationResult>
            ApplyDeltaAsync(
                UserCurrencyType currencyType,
                int deltaAmount,
                string requestId,
                string reason,
                string runId)
        {
            CurrencyLedgerRequest request = new CurrencyLedgerRequest(
                requestId,
                currencyType,
                deltaAmount,
                reason,
                runId,
                DateTimeOffset.UtcNow.ToString(
                    "O",
                    CultureInfo.InvariantCulture));
            if (!request.IsValid)
            {
                return CreateInvalidResult();
            }

            ICurrencyLedgerService activeService = service;
            PendingCurrencyTransactionStore activeStore = pendingStore;
            int activeVersion = configurationVersion;
            if (activeService == null)
            {
                return ApplyLocally(request);
            }

            if (activeStore == null
                || !activeStore.TryEnqueue(
                    request,
                    out CurrencyLedgerRequest storedRequest))
            {
                return new CurrencyLedgerOperationResult(
                    CurrencyLedgerOperationState.Error,
                    GetLocalBalances(),
                    false,
                    "Currency request could not be persisted.");
            }

            request = storedRequest;

            await OperationGate.WaitAsync();
            try
            {
                if (activeVersion != configurationVersion)
                {
                    return CreateQueuedResult(
                        "Currency account changed before processing.");
                }

                if (!activeService.IsOnline)
                {
                    return CreateQueuedResult(
                        "Currency server is offline.");
                }

                CurrencyLedgerOperationResult result =
                    await activeService.ApplyAsync(request);
                if (activeVersion != configurationVersion)
                {
                    return CreateQueuedResult(
                        "Currency account changed while processing.");
                }

                HandleRemoteResult(activeStore, request, result);
                return result.State == CurrencyLedgerOperationState.Error
                    ? CreateQueuedResult(result.Message)
                    : result;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Currency request failed: {exception.Message}");
                return CreateQueuedResult(exception.Message);
            }
            finally
            {
                OperationGate.Release();
            }
        }

        private static async Task FlushPendingRequestsAsync(
            ICurrencyLedgerService activeService,
            PendingCurrencyTransactionStore activeStore,
            int activeVersion)
        {
            if (activeStore == null || activeService == null)
            {
                return;
            }

            IReadOnlyList<CurrencyLedgerRequest> requests =
                activeStore.GetSnapshot();
            for (int i = 0; i < requests.Count; i++)
            {
                if (activeVersion != configurationVersion
                    || !activeService.IsOnline)
                {
                    return;
                }

                CurrencyLedgerRequest request = requests[i];
                CurrencyLedgerOperationResult result =
                    await activeService.ApplyAsync(request);
                if (activeVersion != configurationVersion)
                {
                    return;
                }

                HandleRemoteResult(activeStore, request, result);
                if (result.State == CurrencyLedgerOperationState.Error)
                {
                    Debug.LogWarning(
                        $"Pending currency request failed: {result.Message}");
                    return;
                }
            }
        }

        private static void HandleRemoteResult(
            PendingCurrencyTransactionStore activeStore,
            CurrencyLedgerRequest request,
            CurrencyLedgerOperationResult result)
        {
            if (result.HasAuthoritativeBalances)
            {
                ApplyAuthoritativeBalances(result.Balances);
            }

            if (result.Completed
                || result.State
                == CurrencyLedgerOperationState.InsufficientFunds
                || result.State
                == CurrencyLedgerOperationState.InvalidRequest)
            {
                activeStore?.Remove(request.RequestId);
            }
        }

        private static CurrencyLedgerOperationResult ApplyLocally(
            CurrencyLedgerRequest request)
        {
            UserCurrencyChangeResult result = request.DeltaAmount > 0
                ? UserProfileManager.AddCurrency(
                    request.CurrencyType,
                    request.DeltaAmount)
                : UserProfileManager.TrySpendCurrency(
                    request.CurrencyType,
                    Math.Abs(request.DeltaAmount));
            CurrencyLedgerOperationState state = result.Applied
                ? CurrencyLedgerOperationState.Applied
                : CurrencyLedgerOperationState.InsufficientFunds;
            return new CurrencyLedgerOperationResult(
                state,
                GetLocalBalances(),
                true,
                string.Empty);
        }

        private static void ApplyAuthoritativeBalances(
            CurrencyBalanceSnapshot balances)
        {
            UserProfileManager.ApplyAuthoritativeCurrencyBalances(
                balances.GameMoney,
                balances.Ruby);
        }

        private static CurrencyBalanceSnapshot GetLocalBalances()
        {
            return new CurrencyBalanceSnapshot(
                UserProfileManager.GameMoney,
                UserProfileManager.Ruby);
        }

        private static CurrencyLedgerOperationResult CreateQueuedResult(
            string message)
        {
            return new CurrencyLedgerOperationResult(
                CurrencyLedgerOperationState.Queued,
                GetLocalBalances(),
                false,
                message);
        }

        private static CurrencyLedgerOperationResult CreateInvalidResult()
        {
            return new CurrencyLedgerOperationResult(
                CurrencyLedgerOperationState.InvalidRequest,
                GetLocalBalances(),
                false,
                "Currency request is invalid.");
        }

        private static CurrencySynchronizationResult
            CreateSynchronizationChangedResult(
                CurrencyBalanceSnapshot balances)
        {
            return new CurrencySynchronizationResult(
                false,
                false,
                balances,
                "Currency account changed while synchronizing.");
        }
    }
}
