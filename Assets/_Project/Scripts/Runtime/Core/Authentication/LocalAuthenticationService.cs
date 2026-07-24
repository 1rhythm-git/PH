using System;
using System.Threading.Tasks;
using UnityEngine;

namespace PH.Core.Authentication
{
    public sealed class LocalAuthenticationService : IAuthenticationService
    {
        private const string SaveKey = "PH.Authentication.v1";
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
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
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
                    PlayerPrefs.DeleteKey(SaveKey);
                    PlayerPrefs.Save();
                    return Task.FromResult(AuthenticationResult.Fail(
                        AuthenticationFailure.NoSavedSession,
                        "Saved authentication session was invalid."));
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
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
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
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
                PlayerPrefs.Save();
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
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            return Task.CompletedTask;
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
