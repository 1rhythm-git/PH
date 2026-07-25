using System;
using System.Text;
using UnityEngine;

namespace LootUp.Core.Authentication
{
    public static class LocalLoginCredentialPreferences
    {
        private const string LegacyRememberCredentialsKey =
            "LootUp.Login.RememberCredentials.v1";
        private const string LegacyNicknameKey = "LootUp.Login.Nickname.v1";
        private const string LegacyPasswordKey = "LootUp.Login.Password.v1";
        private const string LegacyAutoLoginKey = "LootUp.Login.AutoLogin.v2";
        private const string LegacyAccountIdKey = "LootUp.Login.AccountId.v2";
        private const string RememberCredentialsKey =
            "LootUp.Login.RememberCredentials.v3";
        private const string AccountIdKey = "LootUp.Login.AccountId.v3";
        private const string PasswordKey = "LootUp.Login.Password.v3";

        public static bool TryLoad(out string accountId, out string password)
        {
            accountId = string.Empty;
            password = string.Empty;
            if (PlayerPrefs.GetInt(RememberCredentialsKey, 0) != 1)
            {
                return false;
            }

            accountId = PlayerPrefs.GetString(AccountIdKey, string.Empty);
            string encodedPassword =
                PlayerPrefs.GetString(PasswordKey, string.Empty);
            try
            {
                password = Encoding.UTF8.GetString(
                    Convert.FromBase64String(encodedPassword));
                return !string.IsNullOrWhiteSpace(accountId)
                       && !string.IsNullOrEmpty(password);
            }
            catch (FormatException)
            {
                Clear();
                accountId = string.Empty;
                password = string.Empty;
                return false;
            }
        }

        public static void Save(string accountId, string password)
        {
            DeleteLegacyCredentials();
            PlayerPrefs.SetInt(RememberCredentialsKey, 1);
            PlayerPrefs.SetString(
                AccountIdKey,
                accountId?.Trim() ?? string.Empty);
            PlayerPrefs.SetString(
                PasswordKey,
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(password ?? string.Empty)));
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            DeleteLegacyCredentials();
            PlayerPrefs.DeleteKey(RememberCredentialsKey);
            PlayerPrefs.DeleteKey(AccountIdKey);
            PlayerPrefs.DeleteKey(PasswordKey);
            PlayerPrefs.Save();
        }

        public static void DeleteLegacyCredentials()
        {
            bool hasLegacyCredentials =
                PlayerPrefs.HasKey(LegacyRememberCredentialsKey)
                || PlayerPrefs.HasKey(LegacyNicknameKey)
                || PlayerPrefs.HasKey(LegacyPasswordKey)
                || PlayerPrefs.HasKey(LegacyAutoLoginKey)
                || PlayerPrefs.HasKey(LegacyAccountIdKey);
            PlayerPrefs.DeleteKey(LegacyRememberCredentialsKey);
            PlayerPrefs.DeleteKey(LegacyNicknameKey);
            PlayerPrefs.DeleteKey(LegacyPasswordKey);
            PlayerPrefs.DeleteKey(LegacyAutoLoginKey);
            PlayerPrefs.DeleteKey(LegacyAccountIdKey);
            if (hasLegacyCredentials)
            {
                PlayerPrefs.Save();
            }
        }
    }
}
