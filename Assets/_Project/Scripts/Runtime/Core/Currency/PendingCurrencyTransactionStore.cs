using System;
using System.Collections.Generic;
using System.Text;
using LootUp.Core.Profile;
using UnityEngine;

namespace LootUp.Core.Currency
{
    internal sealed class PendingCurrencyTransactionStore
    {
        private const int MaximumPendingCount = 128;
        private const string SaveKeyPrefix =
            "LootUp.CurrencyLedger.Pending.v1.";

        private readonly string saveKey;
        private readonly PendingCurrencySaveData saveData;

        public PendingCurrencyTransactionStore(string userId)
        {
            saveKey = CreateSaveKey(userId);
            saveData = Load();
        }

        public bool TryEnqueue(
            CurrencyLedgerRequest request,
            out CurrencyLedgerRequest storedRequest)
        {
            storedRequest = default;
            if (!request.IsValid)
            {
                return false;
            }

            PendingCurrencyRequestData existing = Find(request.RequestId);
            if (existing != null)
            {
                storedRequest = CreateRequest(existing);
                return true;
            }

            if (saveData.Requests.Count >= MaximumPendingCount)
            {
                Debug.LogError("Currency pending queue reached its limit.");
                return false;
            }

            saveData.Requests.Add(new PendingCurrencyRequestData
            {
                RequestId = request.RequestId,
                CurrencyType = request.CurrencyType,
                DeltaAmount = request.DeltaAmount,
                Reason = request.Reason,
                RunId = request.RunId,
                CreatedAt = request.CreatedAt
            });
            storedRequest = request;
            return TrySave();
        }

        public IReadOnlyList<CurrencyLedgerRequest> GetSnapshot()
        {
            List<CurrencyLedgerRequest> result =
                new List<CurrencyLedgerRequest>(saveData.Requests.Count);
            for (int i = 0; i < saveData.Requests.Count; i++)
            {
                PendingCurrencyRequestData entry = saveData.Requests[i];
                if (entry == null)
                {
                    continue;
                }

                result.Add(CreateRequest(entry));
            }

            return result;
        }

        public void Remove(string requestId)
        {
            for (int i = saveData.Requests.Count - 1; i >= 0; i--)
            {
                PendingCurrencyRequestData entry = saveData.Requests[i];
                if (entry != null
                    && string.Equals(
                        entry.RequestId,
                        requestId,
                        StringComparison.Ordinal))
                {
                    saveData.Requests.RemoveAt(i);
                }
            }

            TrySave();
        }

        private PendingCurrencySaveData Load()
        {
            if (string.IsNullOrWhiteSpace(saveKey))
            {
                return new PendingCurrencySaveData();
            }

            string json = PlayerPrefs.GetString(saveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new PendingCurrencySaveData();
            }

            try
            {
                PendingCurrencySaveData loaded =
                    JsonUtility.FromJson<PendingCurrencySaveData>(json)
                    ?? new PendingCurrencySaveData();
                loaded.Requests ??= new List<PendingCurrencyRequestData>();
                return loaded;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Currency pending queue load failed: {exception.Message}");
                return new PendingCurrencySaveData();
            }
        }

        private bool TrySave()
        {
            if (string.IsNullOrWhiteSpace(saveKey))
            {
                return false;
            }

            try
            {
                PlayerPrefs.SetString(
                    saveKey,
                    JsonUtility.ToJson(saveData));
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Currency pending queue save failed: {exception.Message}");
                return false;
            }
        }

        private PendingCurrencyRequestData Find(string requestId)
        {
            for (int i = 0; i < saveData.Requests.Count; i++)
            {
                PendingCurrencyRequestData entry = saveData.Requests[i];
                if (entry != null
                    && string.Equals(
                        entry.RequestId,
                        requestId,
                        StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        private static CurrencyLedgerRequest CreateRequest(
            PendingCurrencyRequestData entry)
        {
            return new CurrencyLedgerRequest(
                entry.RequestId,
                entry.CurrencyType,
                entry.DeltaAmount,
                entry.Reason,
                entry.RunId,
                entry.CreatedAt);
        }

        private static string CreateSaveKey(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return string.Empty;
            }

            string encodedUserId = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(userId.Trim()))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return SaveKeyPrefix + encodedUserId;
        }

        [Serializable]
        private sealed class PendingCurrencySaveData
        {
            public int Version = 1;
            public List<PendingCurrencyRequestData> Requests =
                new List<PendingCurrencyRequestData>();
        }

        [Serializable]
        private sealed class PendingCurrencyRequestData
        {
            public string RequestId;
            public UserCurrencyType CurrencyType;
            public int DeltaAmount;
            public string Reason;
            public string RunId;
            public string CreatedAt;
        }
    }
}
