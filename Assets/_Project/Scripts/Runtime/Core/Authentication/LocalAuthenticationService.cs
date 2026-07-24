using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LootUp.Core.Authentication
{
    public sealed class LocalAuthenticationService : IAuthenticationService
    {
        private const string SaveKey = "LootUp.Authentication.v1";
        private const string LegacySaveKey = "PH.Authentication.v1";
        private const string GuestIdPrefix = "guest-";
        private const string DefaultNickname = "Player";

        private readonly string fallbackUserId;
        private readonly string fallbackNickname;

        public LocalAuthenticationService(string fallbackUserId, string fallbackNickname)
        {
            this.fallbackUserId = string.IsNullOrWhiteSpace(fallbackUserId) ? string.Empty : fallbackUserId.Trim();
            this.fallbackNickname = string.IsNullOrWhiteSpace(fallbackNickname) ? DefaultNickname : fallbackNickname.Trim();
        }

        public Task<AuthenticationResult> TryRestoreSessionAsync()
        {
            string json = GetSavedSessionJson(out bool loadedFromLegacyKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Task.FromResult(AuthenticationResult.Fail(
                    AuthenticationFailure.NoSavedSession,
                    "Saved authentication session was not found."));
            }

            try
            {
                LocalAuthenticationSaveData saveData = JsonUtility.FromJson<LocalAuthenticationSaveData>(json);
                if (saveData == null || string.IsNullOrWhiteSpace(saveData.UserId))
                {
                    DeleteSavedSession();
                    return Task.FromResult(AuthenticationResult.Fail(
                        AuthenticationFailure.NoSavedSession,
                        "Saved authentication session was invalid."));
                }

                if (loadedFromLegacyKey)
                {
                    SaveSessionJson(json);
                }

                AuthenticationSession session = new AuthenticationSession(
                    saveData.UserId,
                    saveData.Nickname,
                    AuthenticationProvider.LocalGuest,
                    true);
                return Task.FromResult(AuthenticationResult.Success(session));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Authentication session restore failed: {exception.Message}");
                DeleteSavedSession();
                return Task.FromResult(AuthenticationResult.Fail(
                    AuthenticationFailure.NoSavedSession,
                    "Saved authentication session could not be read."));
            }
        }

        public Task<AuthenticationResult> SignInAsGuestAsync()
        {
            string userId = !string.IsNullOrWhiteSpace(fallbackUserId)
                ? fallbackUserId
                : GuestIdPrefix + Guid.NewGuid().ToString("N");
            AuthenticationSession session = new AuthenticationSession(
                userId,
                fallbackNickname,
                AuthenticationProvider.LocalGuest,
                true);

            try
            {
                LocalAuthenticationSaveData saveData = new LocalAuthenticationSaveData
                {
                    UserId = session.UserId,
                    Nickname = session.Nickname
                };
                SaveSessionJson(JsonUtility.ToJson(saveData));
                return Task.FromResult(AuthenticationResult.Success(session));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Guest authentication save failed: {exception.Message}");
                return Task.FromResult(AuthenticationResult.Fail(
                    AuthenticationFailure.Unexpected,
                    "Guest authentication session could not be saved."));
            }
        }

        public Task<AuthenticationResult> SignInAsync(string accountId, string password)
        {
            // 계정 로그인은 BackND 인증 구현체가 연결된 뒤 활성화한다.
            return Task.FromResult(AuthenticationResult.Fail(
                AuthenticationFailure.ProviderUnavailable,
                "Account authentication requires a server provider."));
        }

        public Task SignOutAsync()
        {
            DeleteSavedSession();
            return Task.CompletedTask;
        }

        private static string GetSavedSessionJson(out bool loadedFromLegacyKey)
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            loadedFromLegacyKey = string.IsNullOrWhiteSpace(json);
            return loadedFromLegacyKey
                ? PlayerPrefs.GetString(LegacySaveKey, string.Empty)
                : json;
        }

        private static void SaveSessionJson(string json)
        {
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.DeleteKey(LegacySaveKey);
            PlayerPrefs.Save();
        }

        private static void DeleteSavedSession()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(LegacySaveKey);
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class LocalAuthenticationSaveData
        {
            public int Version = 1;
            public string UserId;
            public string Nickname = DefaultNickname;
        }
    }
}
