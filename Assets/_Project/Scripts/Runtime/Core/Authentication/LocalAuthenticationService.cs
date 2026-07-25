using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LootUp.Core.Authentication
{
    public sealed class LocalAuthenticationService : IAuthenticationService
    {
        private const string SessionSaveKey = "LootUp.Authentication.v2";
        private const string LegacySessionSaveKey = "LootUp.Authentication.v1";
        private const string LegacyPhSessionSaveKey = "PH.Authentication.v1";
        private const string GuestAccountsSaveKey = "LootUp.GuestAccounts.v1";
        private const string GuestIdPrefix = "guest-";
        private const int PasswordIterationCount = 10000;
        private const int PasswordSaltByteCount = 16;
        private const int PasswordHashByteCount = 32;
        private const int NicknameMinimumLength = 2;
        private const int NicknameMaximumLength = 12;
        private const int PasswordMinimumLength = 6;
        private const int PasswordMaximumLength = 32;

        public LocalAuthenticationService(string fallbackUserId, string fallbackNickname)
        {
            // 기존 생성자 호출부 호환을 유지하며 신규 Guest는 입력한 자격 증명만 사용한다.
        }

        public Task<AuthenticationResult> TryRestoreSessionAsync()
        {
            string json = PlayerPrefs.GetString(SessionSaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                DeleteLegacySessions();
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.NoSavedSession,
                        "Saved authentication session was not found."));
            }

            try
            {
                LocalAuthenticationSessionSaveData saveData =
                    JsonUtility.FromJson<LocalAuthenticationSessionSaveData>(json);
                GuestAccountSaveData account = FindAccountByUserId(saveData?.UserId);
                if (account == null)
                {
                    DeleteSavedSession();
                    return Task.FromResult(
                        AuthenticationResult.Fail(
                            AuthenticationFailure.NoSavedSession,
                            "Saved Guest account was not found."));
                }

                return Task.FromResult(
                    AuthenticationResult.Success(CreateSession(account)));
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Authentication session restore failed: {exception.Message}");
                DeleteSavedSession();
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.NoSavedSession,
                        "Saved authentication session could not be read."));
            }
        }

        public Task<NicknameAvailabilityResult> CheckNicknameAvailabilityAsync(
            string nickname)
        {
            if (!TryNormalizeNickname(
                nickname,
                out string normalizedNickname,
                out string validationMessage))
            {
                return Task.FromResult(
                    NicknameAvailabilityResult.Unavailable(
                        AuthenticationFailure.InvalidNickname,
                        validationMessage));
            }

            LocalGuestAccountsSaveData accounts = LoadGuestAccounts();
            if (FindAccountByNickname(accounts, normalizedNickname) != null)
            {
                return Task.FromResult(
                    NicknameAvailabilityResult.Unavailable(
                        AuthenticationFailure.NicknameAlreadyExists,
                        "Nickname is already in use."));
            }

            if (HasReachedGuestAccountLimit(accounts))
            {
                return Task.FromResult(
                    NicknameAvailabilityResult.Unavailable(
                        AuthenticationFailure.GuestAccountLimitReached,
                        "This Android device already has a Guest account."));
            }

            return Task.FromResult(NicknameAvailabilityResult.Available());
        }

        public Task<AuthenticationResult> RegisterGuestAsync(
            string nickname,
            string password)
        {
            if (!TryValidateCredentials(
                nickname,
                password,
                out string normalizedNickname,
                out AuthenticationFailure failure,
                out string validationMessage))
            {
                return Task.FromResult(
                    AuthenticationResult.Fail(failure, validationMessage));
            }

            LocalGuestAccountsSaveData accounts = LoadGuestAccounts();
            if (FindAccountByNickname(accounts, normalizedNickname) != null)
            {
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.NicknameAlreadyExists,
                        "Nickname is already in use."));
            }

            if (HasReachedGuestAccountLimit(accounts))
            {
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.GuestAccountLimitReached,
                        "This Android device already has a Guest account."));
            }

            try
            {
                byte[] salt = CreatePasswordSalt();
                GuestAccountSaveData account = new GuestAccountSaveData
                {
                    UserId = GuestIdPrefix + Guid.NewGuid().ToString("N"),
                    Nickname = normalizedNickname,
                    PasswordSalt = Convert.ToBase64String(salt),
                    PasswordHash = HashPassword(password, salt),
                    CreatedAtUnixMilliseconds =
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                accounts.Accounts.Add(account);
                SaveGuestAccounts(accounts);
                SaveSession(account);
                return Task.FromResult(
                    AuthenticationResult.Success(CreateSession(account)));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Guest registration failed: {exception.Message}");
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.Unexpected,
                        "Guest account could not be saved."));
            }
        }

        public Task<AuthenticationResult> SignInGuestAsync(
            string nickname,
            string password)
        {
            if (!TryNormalizeNickname(
                nickname,
                out string normalizedNickname,
                out string nicknameMessage))
            {
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.InvalidNickname,
                        nicknameMessage));
            }

            if (string.IsNullOrEmpty(password))
            {
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.InvalidCredentials,
                        "Password is required."));
            }

            GuestAccountSaveData account = FindAccountByNickname(normalizedNickname);
            if (account == null)
            {
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.NicknameNotFound,
                        "Guest nickname was not found."));
            }

            try
            {
                byte[] salt = Convert.FromBase64String(account.PasswordSalt);
                string passwordHash = HashPassword(password, salt);
                if (!FixedTimeEquals(account.PasswordHash, passwordHash))
                {
                    return Task.FromResult(
                        AuthenticationResult.Fail(
                            AuthenticationFailure.InvalidCredentials,
                            "Password does not match."));
                }

                SaveSession(account);
                return Task.FromResult(
                    AuthenticationResult.Success(CreateSession(account)));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Guest login failed: {exception.Message}");
                return Task.FromResult(
                    AuthenticationResult.Fail(
                        AuthenticationFailure.Unexpected,
                        "Guest account could not be read."));
            }
        }

        public Task<AuthenticationResult> SignInAsGuestAsync()
        {
            return Task.FromResult(
                AuthenticationResult.Fail(
                    AuthenticationFailure.InvalidCredentials,
                    "Guest nickname and password are required."));
        }

        public Task<AuthenticationResult> SignInAsync(
            string accountId,
            string password)
        {
            // Google 회원가입은 BackND 인증 구현체가 연결된 뒤 활성화한다.
            return Task.FromResult(
                AuthenticationResult.Fail(
                    AuthenticationFailure.ProviderUnavailable,
                    "Google registration requires BackND."));
        }

        public Task SignOutAsync()
        {
            DeleteSavedSession();
            return Task.CompletedTask;
        }

        private static bool TryValidateCredentials(
            string nickname,
            string password,
            out string normalizedNickname,
            out AuthenticationFailure failure,
            out string message)
        {
            if (!TryNormalizeNickname(nickname, out normalizedNickname, out message))
            {
                failure = AuthenticationFailure.InvalidNickname;
                return false;
            }

            if (string.IsNullOrEmpty(password)
                || password.Length < PasswordMinimumLength
                || password.Length > PasswordMaximumLength)
            {
                failure = AuthenticationFailure.WeakPassword;
                message =
                    $"Password must be {PasswordMinimumLength}-{PasswordMaximumLength} characters.";
                return false;
            }

            failure = AuthenticationFailure.None;
            message = string.Empty;
            return true;
        }

        private static bool TryNormalizeNickname(
            string nickname,
            out string normalizedNickname,
            out string message)
        {
            normalizedNickname = string.IsNullOrWhiteSpace(nickname)
                ? string.Empty
                : nickname.Trim().Normalize(NormalizationForm.FormKC);
            if (normalizedNickname.Length < NicknameMinimumLength
                || normalizedNickname.Length > NicknameMaximumLength)
            {
                message =
                    $"Nickname must be {NicknameMinimumLength}-{NicknameMaximumLength} characters.";
                return false;
            }

            for (int i = 0; i < normalizedNickname.Length; i++)
            {
                char character = normalizedNickname[i];
                if (!char.IsLetterOrDigit(character) && character != '_')
                {
                    message = "Nickname can use letters, numbers, and underscore.";
                    return false;
                }
            }

            message = string.Empty;
            return true;
        }

        private static byte[] CreatePasswordSalt()
        {
            byte[] salt = new byte[PasswordSaltByteCount];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            return salt;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(
                password,
                salt,
                PasswordIterationCount,
                HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(
                    deriveBytes.GetBytes(PasswordHashByteCount));
            }
        }

        private static bool FixedTimeEquals(string expected, string actual)
        {
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expected ?? string.Empty);
            byte[] actualBytes = Encoding.UTF8.GetBytes(actual ?? string.Empty);
            int difference = expectedBytes.Length ^ actualBytes.Length;
            int length = Math.Min(expectedBytes.Length, actualBytes.Length);
            for (int i = 0; i < length; i++)
            {
                difference |= expectedBytes[i] ^ actualBytes[i];
            }

            return difference == 0;
        }

        private static AuthenticationSession CreateSession(
            GuestAccountSaveData account)
        {
            return new AuthenticationSession(
                account.UserId,
                account.Nickname,
                AuthenticationProvider.LocalGuest,
                true);
        }

        private static GuestAccountSaveData FindAccountByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            LocalGuestAccountsSaveData accounts = LoadGuestAccounts();
            for (int i = 0; i < accounts.Accounts.Count; i++)
            {
                GuestAccountSaveData account = accounts.Accounts[i];
                if (account != null
                    && string.Equals(
                        account.UserId,
                        userId,
                        StringComparison.Ordinal))
                {
                    return account;
                }
            }

            return null;
        }

        private static GuestAccountSaveData FindAccountByNickname(string nickname)
        {
            return FindAccountByNickname(LoadGuestAccounts(), nickname);
        }

        private static GuestAccountSaveData FindAccountByNickname(
            LocalGuestAccountsSaveData accounts,
            string nickname)
        {
            for (int i = 0; i < accounts.Accounts.Count; i++)
            {
                GuestAccountSaveData account = accounts.Accounts[i];
                if (account != null
                    && string.Equals(
                        account.Nickname,
                        nickname,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return account;
                }
            }

            return null;
        }

        private static bool HasReachedGuestAccountLimit(
            LocalGuestAccountsSaveData accounts)
        {
            // Editor에서는 빌드 타깃과 관계없이 다중 계정 테스트를 허용한다.
            if (Application.isEditor
                || Application.platform != RuntimePlatform.Android)
            {
                return false;
            }

            // Android 실제 기기에서는 설치 데이터당 Guest 계정을 하나만 허용한다.
            for (int i = 0; i < accounts.Accounts.Count; i++)
            {
                if (accounts.Accounts[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static LocalGuestAccountsSaveData LoadGuestAccounts()
        {
            string json = PlayerPrefs.GetString(GuestAccountsSaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new LocalGuestAccountsSaveData();
            }

            try
            {
                LocalGuestAccountsSaveData accounts =
                    JsonUtility.FromJson<LocalGuestAccountsSaveData>(json)
                    ?? new LocalGuestAccountsSaveData();
                accounts.Accounts ??= new List<GuestAccountSaveData>();
                return accounts;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Guest account list load failed: {exception.Message}");
                return new LocalGuestAccountsSaveData();
            }
        }

        private static void SaveGuestAccounts(LocalGuestAccountsSaveData accounts)
        {
            PlayerPrefs.SetString(
                GuestAccountsSaveKey,
                JsonUtility.ToJson(accounts));
            PlayerPrefs.Save();
        }

        private static void SaveSession(GuestAccountSaveData account)
        {
            LocalAuthenticationSessionSaveData saveData =
                new LocalAuthenticationSessionSaveData
                {
                    UserId = account.UserId,
                    Nickname = account.Nickname
                };
            PlayerPrefs.SetString(
                SessionSaveKey,
                JsonUtility.ToJson(saveData));
            DeleteLegacySessions();
            PlayerPrefs.Save();
        }

        private static void DeleteSavedSession()
        {
            PlayerPrefs.DeleteKey(SessionSaveKey);
            DeleteLegacySessions();
            PlayerPrefs.Save();
        }

        private static void DeleteLegacySessions()
        {
            PlayerPrefs.DeleteKey(LegacySessionSaveKey);
            PlayerPrefs.DeleteKey(LegacyPhSessionSaveKey);
        }

        [Serializable]
        private sealed class LocalAuthenticationSessionSaveData
        {
            public int Version = 2;
            public string UserId;
            public string Nickname;
        }

        [Serializable]
        private sealed class LocalGuestAccountsSaveData
        {
            public int Version = 1;
            public List<GuestAccountSaveData> Accounts =
                new List<GuestAccountSaveData>();
        }

        [Serializable]
        private sealed class GuestAccountSaveData
        {
            public string UserId;
            public string Nickname;
            public string PasswordSalt;
            public string PasswordHash;
            public long CreatedAtUnixMilliseconds;
        }
    }
}
