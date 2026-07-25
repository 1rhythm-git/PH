using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BackEnd;
using BackEnd.Leaderboard;
using LitJson;
using LootUp.Core.Backend;
using UnityEngine;
using BackndApi = BackEnd.Backend;

namespace LootUp.Core.Leaderboard
{
    public sealed class BackndLeaderboardService : ILeaderboardService
    {
        public const string TableName = "LootUpRank";
        public const string LifetimeBestTableName = "LootUpBest";
        public const string RankColumnName = "rankValue";
        public const string ExtraDataColumnName = "recordData";

        private const int MaximumFloor = 9999;
        private const int MaximumScore = 99999999;
        private const int MaximumCharacterLevel = 999;
        private const long ScoreMultiplier = 1000L;
        private const long FloorMultiplier = 100000000000L;

        private readonly string userId;
        private string rankUuid;

        public BackndLeaderboardService(string userId)
        {
            this.userId = userId ?? string.Empty;
        }

        public bool IsOnline =>
            !string.IsNullOrWhiteSpace(userId)
            && BackndSdkManager.State
            == BackndInitializationState.Initialized;

        public async Task<LeaderboardSnapshot> LoadAsync(int limit)
        {
            string configurationError = await EnsureRankConfigurationAsync();
            if (!string.IsNullOrEmpty(configurationError))
            {
                Debug.LogWarning(
                    $"Leaderboard configuration failed: {configurationError}");
                return new LeaderboardSnapshot(
                    LeaderboardLoadState.Error,
                    Array.Empty<LeaderboardRecord>(),
                    null,
                    configurationError);
            }

            BackendUserLeaderboardReturnObject listResponse =
                await GetLeaderboardAsync(
                    Math.Min(50, Math.Max(1, limit)));
            if (!listResponse.IsSuccess())
            {
                return CreateErrorSnapshot(listResponse);
            }

            List<LeaderboardRecord> records =
                ParseRankRecords(
                    listResponse.GetUserLeaderboardList());
            LeaderboardRecord myRecord = FindRecord(records, userId);
            if (myRecord == null)
            {
                BackendUserLeaderboardReturnObject myRankResponse =
                    await GetMyLeaderboardAsync();
                if (myRankResponse.IsSuccess())
                {
                    List<LeaderboardRecord> myRecords =
                        ParseRankRecords(
                            myRankResponse.GetUserLeaderboardList());
                    myRecord = FindRecord(myRecords, userId);
                    if (myRecord == null && myRecords.Count > 0)
                    {
                        myRecord = myRecords[0];
                    }
                }
            }

            return new LeaderboardSnapshot(
                LeaderboardLoadState.Online,
                records,
                myRecord,
                string.Empty);
        }

        public async Task<LeaderboardRecord>
            SynchronizeLifetimeBestAsync(
                LeaderboardRecord localRecord)
        {
            StoredRecordQuery lifetimeQuery =
                await GetStoredRecordAsync(
                    LifetimeBestTableName,
                    localRecord?.Nickname ?? string.Empty);
            if (!lifetimeQuery.Response.IsSuccess())
            {
                Debug.LogWarning(
                    $"Lifetime best load failed: {lifetimeQuery.Response}");
                return localRecord;
            }

            LeaderboardRecord bestRecord = SelectBetterRecord(
                lifetimeQuery.Record,
                localRecord);
            if (lifetimeQuery.Record == null)
            {
                StoredRecordQuery legacyQuery =
                    await GetStoredRecordAsync(
                        TableName,
                        localRecord?.Nickname ?? string.Empty);
                if (legacyQuery.Response.IsSuccess())
                {
                    bestRecord = SelectBetterRecord(
                        bestRecord,
                        legacyQuery.Record);
                }
            }

            if (bestRecord == null)
            {
                return null;
            }

            bool shouldSave =
                lifetimeQuery.Record == null
                || CompareRecords(
                    bestRecord,
                    lifetimeQuery.Record) > 0;
            if (!shouldSave)
            {
                return lifetimeQuery.Record;
            }

            Param param = CreateParam(bestRecord);
            BackendReturnObject saveResponse;
            if (string.IsNullOrWhiteSpace(
                    lifetimeQuery.RowInDate))
            {
                saveResponse = await RunRequest(
                    callback => BackndApi.GameData.Insert(
                        LifetimeBestTableName,
                        param,
                        callback));
            }
            else
            {
                saveResponse = await RunRequest(
                    callback => BackndApi.GameData.UpdateV2(
                        LifetimeBestTableName,
                        lifetimeQuery.RowInDate,
                        userId,
                        param,
                        callback));
            }

            if (!saveResponse.IsSuccess())
            {
                Debug.LogWarning(
                    $"Lifetime best update failed: {saveResponse}");
            }

            return bestRecord;
        }

        public async Task<LeaderboardSubmitResult> SubmitAsync(
            LeaderboardRecord record)
        {
            if (record == null || !record.HasRecord)
            {
                return LeaderboardSubmitResult.Success(false);
            }

            await SynchronizeLifetimeBestAsync(record);

            string configurationError = await EnsureRankConfigurationAsync();
            if (!string.IsNullOrEmpty(configurationError))
            {
                Debug.LogWarning(
                    $"Leaderboard configuration failed: {configurationError}");
                return LeaderboardSubmitResult.Fail(configurationError);
            }

            BackendReturnObject dataResponse = await RunRequest(
                callback => BackndApi.GameData.GetMyData(
                    TableName,
                    new Where(),
                    1,
                    callback));
            if (!dataResponse.IsSuccess())
            {
                return CreateSubmitFailure(dataResponse);
            }

            JsonData rows = dataResponse.FlattenRows();
            string rowInDate = string.Empty;
            LeaderboardRecord savedRecord = null;
            if (rows != null && rows.Count > 0)
            {
                JsonData row = rows[0];
                rowInDate = GetString(row, "inDate");
                savedRecord = ParseStoredRecord(row, record.Nickname);
            }

            bool savedRecordIsBetterOrEqual =
                savedRecord != null
                && CompareRecords(savedRecord, record) >= 0;
            if (savedRecordIsBetterOrEqual)
            {
                BackendUserLeaderboardReturnObject currentRankResponse =
                    await GetMyLeaderboardAsync();
                if (currentRankResponse.IsSuccess())
                {
                    List<LeaderboardRecord> currentRankRecords =
                        ParseRankRecords(
                            currentRankResponse.GetUserLeaderboardList());
                    if (FindRecord(currentRankRecords, userId) != null
                        || currentRankRecords.Count > 0)
                    {
                        return LeaderboardSubmitResult.Success(false);
                    }
                }
                else if (!IsMissingCurrentLeaderboardRecord(
                             currentRankResponse))
                {
                    return CreateSubmitFailure(currentRankResponse);
                }
            }

            Param param = CreateParam(record);
            if (string.IsNullOrWhiteSpace(rowInDate))
            {
                BackendReturnObject insertResponse = await RunRequest(
                    callback => BackndApi.GameData.Insert(
                        TableName,
                        param,
                        callback));
                if (!insertResponse.IsSuccess())
                {
                    return CreateSubmitFailure(insertResponse);
                }

                rowInDate = insertResponse.GetInDate();
            }

            if (string.IsNullOrWhiteSpace(rowInDate))
            {
                return LeaderboardSubmitResult.Fail(
                    "BackND ranking row inDate was not returned.");
            }

            BackendReturnObject rankResponse =
                await UpdateLeaderboardAsync(
                    rowInDate,
                    param);
            return rankResponse.IsSuccess()
                ? LeaderboardSubmitResult.Success(true)
                : CreateSubmitFailure(rankResponse);
        }

        private async Task<string> EnsureRankConfigurationAsync()
        {
            if (!string.IsNullOrWhiteSpace(rankUuid))
            {
                return string.Empty;
            }

            BackendLeaderboardTableReturnObject response =
                await GetLeaderboardsAsync();
            if (!response.IsSuccess())
            {
                return GetResponseMessage(
                    response,
                    "BackND ranking configuration could not be loaded.");
            }

            List<LeaderboardTableItem> leaderboards =
                response.GetLeaderboardTableList();
            for (int i = 0;
                 leaderboards != null && i < leaderboards.Count;
                 i++)
            {
                LeaderboardTableItem leaderboard = leaderboards[i];
                if (!string.Equals(
                        leaderboard.table,
                        TableName,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        leaderboard.column,
                        RankColumnName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(
                        leaderboard.order,
                        "desc",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return "BackND ranking must use descending order.";
                }

                if (!string.Equals(
                        leaderboard.extraDataColumn,
                        ExtraDataColumnName,
                        StringComparison.Ordinal))
                {
                    return
                        $"BackND ranking extra column must be {ExtraDataColumnName}.";
                }

                rankUuid = leaderboard.uuid;
                return string.IsNullOrWhiteSpace(rankUuid)
                    ? "BackND ranking UUID was not returned."
                    : string.Empty;
            }

            return
                $"Create a BackND user ranking for {TableName}.{RankColumnName}.";
        }

        private static Param CreateParam(LeaderboardRecord record)
        {
            int floor = Math.Min(MaximumFloor, record.HighestFloor);
            int score = Math.Min(MaximumScore, record.Score);
            int level = Math.Min(
                MaximumCharacterLevel,
                record.CharacterLevel);
            LeaderboardRecordPayload payload =
                new LeaderboardRecordPayload
                {
                    f = floor,
                    s = score,
                    l = level,
                    c = record.CharacterId ?? string.Empty
                };

            Param param = new Param();
            param.Add(
                RankColumnName,
                (double)ComposeRankValue(floor, score, level));
            param.Add("highestFloor", floor);
            param.Add("score", score);
            param.Add("characterLevel", level);
            param.Add("characterId", payload.c);
            param.Add(
                ExtraDataColumnName,
                JsonUtility.ToJson(payload));
            param.Add("recordVersion", payload.v);
            return param;
        }

        private static long ComposeRankValue(
            int highestFloor,
            int score,
            int characterLevel)
        {
            return (long)highestFloor * FloorMultiplier
                   + (long)score * ScoreMultiplier
                   + characterLevel;
        }

        private static List<LeaderboardRecord> ParseRankRecords(
            IReadOnlyList<UserLeaderboardItem> items)
        {
            List<LeaderboardRecord> records =
                new List<LeaderboardRecord>();
            if (items == null)
            {
                return records;
            }

            for (int i = 0; i < items.Count; i++)
            {
                UserLeaderboardItem item = items[i];
                long rankValue = ParseLong(
                    item.score);
                LeaderboardRecordPayload payload =
                    ParsePayload(
                        item.extraData,
                        rankValue);
                records.Add(
                    new LeaderboardRecord(
                        ParseInt(item.rank),
                        item.gamerInDate,
                        item.nickname,
                        payload.f,
                        payload.s,
                        payload.c,
                        payload.l));
            }

            return records;
        }

        private static LeaderboardRecord ParseStoredRecord(
            JsonData row,
            string nickname)
        {
            if (row == null)
            {
                return null;
            }

            LeaderboardRecordPayload payload = ParsePayload(
                GetString(row, ExtraDataColumnName),
                ParseLong(GetString(row, RankColumnName)));
            return new LeaderboardRecord(
                0,
                GetString(row, "owner_inDate"),
                nickname,
                payload.f,
                payload.s,
                payload.c,
                payload.l);
        }

        private static LeaderboardRecordPayload ParsePayload(
            string json,
            long rankValue)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    LeaderboardRecordPayload payload =
                        JsonUtility.FromJson<LeaderboardRecordPayload>(json);
                    if (payload != null)
                    {
                        payload.f = Math.Max(0, payload.f);
                        payload.s = Math.Max(0, payload.s);
                        payload.l = Math.Max(1, payload.l);
                        payload.c ??= string.Empty;
                        return payload;
                    }
                }
                catch
                {
                }
            }

            long normalized = Math.Max(0L, rankValue);
            int floor = (int)(normalized / FloorMultiplier);
            long remainder = normalized % FloorMultiplier;
            int score = (int)(remainder / ScoreMultiplier);
            int level = (int)(remainder % ScoreMultiplier);
            return new LeaderboardRecordPayload
            {
                f = floor,
                s = score,
                l = Math.Max(1, level)
            };
        }

        private static int CompareRecords(
            LeaderboardRecord left,
            LeaderboardRecord right)
        {
            int floorComparison =
                left.HighestFloor.CompareTo(right.HighestFloor);
            if (floorComparison != 0)
            {
                return floorComparison;
            }

            int scoreComparison = left.Score.CompareTo(right.Score);
            return scoreComparison != 0
                ? scoreComparison
                : left.CharacterLevel.CompareTo(right.CharacterLevel);
        }

        private static LeaderboardRecord SelectBetterRecord(
            LeaderboardRecord left,
            LeaderboardRecord right)
        {
            if (left == null)
            {
                return right;
            }

            if (right == null)
            {
                return left;
            }

            return CompareRecords(left, right) >= 0
                ? left
                : right;
        }

        private static LeaderboardRecord FindRecord(
            IReadOnlyList<LeaderboardRecord> records,
            string targetUserId)
        {
            for (int i = 0; i < records.Count; i++)
            {
                if (string.Equals(
                    records[i].UserId,
                    targetUserId,
                    StringComparison.Ordinal))
                {
                    return records[i];
                }
            }

            return null;
        }

        private static string GetString(JsonData data, string key)
        {
            return data != null
                   && data.IsObject
                   && data.ContainsKey(key)
                   && data[key] != null
                ? data[key].ToString()
                : string.Empty;
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int result)
                ? result
                : 0;
        }

        private static long ParseLong(string value)
        {
            if (long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long integerResult))
            {
                return integerResult;
            }

            return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double doubleResult)
                ? (long)Math.Round(doubleResult)
                : 0L;
        }

        private static LeaderboardSnapshot CreateErrorSnapshot(
            BackendReturnObject response)
        {
            string message = GetResponseMessage(
                response,
                "BackND leaderboard could not be loaded.");
            Debug.LogWarning($"Leaderboard load failed: {response}");
            return new LeaderboardSnapshot(
                LeaderboardLoadState.Error,
                Array.Empty<LeaderboardRecord>(),
                null,
                message);
        }

        private static LeaderboardSubmitResult CreateSubmitFailure(
            BackendReturnObject response)
        {
            Debug.LogWarning($"Leaderboard update failed: {response}");
            return LeaderboardSubmitResult.Fail(
                GetResponseMessage(
                    response,
                    "BackND ranking could not be updated."));
        }

        private static bool IsMissingCurrentLeaderboardRecord(
            BackendReturnObject response)
        {
            if (response == null)
            {
                return false;
            }

            string errorText = string.Concat(
                response.GetErrorCode(),
                " ",
                response.GetMessage(),
                " ",
                response.GetErrorMessage());
            return errorText.IndexOf(
                       "userRank",
                       StringComparison.OrdinalIgnoreCase) >= 0
                   && errorText.IndexOf(
                       "not found",
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetResponseMessage(
            BackendReturnObject response,
            string fallback)
        {
            string message = response != null
                ? response.GetMessage()
                : string.Empty;
            return string.IsNullOrWhiteSpace(message)
                ? fallback
                : message;
        }

        private static Task<BackendReturnObject> RunRequest(
            Action<BackndApi.BackendCallback> request)
        {
            TaskCompletionSource<BackendReturnObject> source = new();
            try
            {
                request(response =>
                    BackndSdkManager.PostToMainThread(
                        () => source.TrySetResult(response)));
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
            }

            return source.Task;
        }

        private async Task<StoredRecordQuery> GetStoredRecordAsync(
            string tableName,
            string nickname)
        {
            BackendReturnObject response = await RunRequest(
                callback => BackndApi.GameData.GetMyData(
                    tableName,
                    new Where(),
                    1,
                    callback));
            if (!response.IsSuccess())
            {
                return new StoredRecordQuery(
                    response,
                    string.Empty,
                    null);
            }

            JsonData rows = response.FlattenRows();
            if (rows == null || rows.Count <= 0)
            {
                return new StoredRecordQuery(
                    response,
                    string.Empty,
                    null);
            }

            JsonData row = rows[0];
            return new StoredRecordQuery(
                response,
                GetString(row, "inDate"),
                ParseStoredRecord(row, nickname));
        }

        private Task<BackendLeaderboardTableReturnObject>
            GetLeaderboardsAsync()
        {
            TaskCompletionSource<BackendLeaderboardTableReturnObject>
                source = new();
            try
            {
                BackndApi.Leaderboard.User.GetLeaderboards(
                    response => BackndSdkManager.PostToMainThread(
                        () => source.TrySetResult(response)));
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
            }

            return source.Task;
        }

        private Task<BackendUserLeaderboardReturnObject>
            GetLeaderboardAsync(int limit)
        {
            TaskCompletionSource<BackendUserLeaderboardReturnObject>
                source = new();
            try
            {
                BackndApi.Leaderboard.User.GetLeaderboard(
                    rankUuid,
                    limit,
                    response => BackndSdkManager.PostToMainThread(
                        () => source.TrySetResult(response)));
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
            }

            return source.Task;
        }

        private Task<BackendUserLeaderboardReturnObject>
            GetMyLeaderboardAsync()
        {
            TaskCompletionSource<BackendUserLeaderboardReturnObject>
                source = new();
            try
            {
                BackndApi.Leaderboard.User.GetMyLeaderboard(
                    rankUuid,
                    response => BackndSdkManager.PostToMainThread(
                        () => source.TrySetResult(response)));
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
            }

            return source.Task;
        }

        private Task<BackendReturnObject> UpdateLeaderboardAsync(
            string rowInDate,
            Param param)
        {
            TaskCompletionSource<BackendReturnObject> source = new();
            try
            {
                BackndApi.Leaderboard.User
                    .UpdateMyDataAndRefreshLeaderboard(
                        rankUuid,
                        TableName,
                        rowInDate,
                        param,
                        response =>
                            BackndSdkManager.PostToMainThread(
                                () => source.TrySetResult(response)));
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
            }

            return source.Task;
        }

        private sealed class StoredRecordQuery
        {
            public StoredRecordQuery(
                BackendReturnObject response,
                string rowInDate,
                LeaderboardRecord record)
            {
                Response = response;
                RowInDate = rowInDate ?? string.Empty;
                Record = record;
            }

            public BackendReturnObject Response { get; }
            public string RowInDate { get; }
            public LeaderboardRecord Record { get; }
        }
    }
}
