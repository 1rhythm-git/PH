using System;
using System.Text;
using UnityEngine;

namespace LootUp.Core.Authentication
{
    public static class LocalLoginCredentialPreferences
    {
        private const string RememberCredentialsKey =
            "LootUp.Login.RememberCredentials.v1";
        private const string NicknameKey = "LootUp.Login.Nickname.v1";
        private const string PasswordKey = "LootUp.Login.Password.v1";

        public static bool IsEnabled =>
            PlayerPrefs.GetInt(RememberCredentialsKey, 0) == 1;

        public static bool TryLoad(out string nickname, out string password)
        {
            nickname = string.Empty;
            password = string.Empty;
            if (!IsEnabled)
            {
                return false;
            }

            nickname = PlayerPrefs.GetString(NicknameKey, string.Empty);
            string encodedPassword =
                PlayerPrefs.GetString(PasswordKey, string.Empty);
            try
            {
                password = Encoding.UTF8.GetString(
                    Convert.FromBase64String(encodedPassword));
                return !string.IsNullOrWhiteSpace(nickname)
                    && !string.IsNullOrEmpty(password);
            }
            catch (FormatException)
            {
                Clear();
                return false;
            }
        }

        public static void Save(string nickname, string password)
        {
            PlayerPrefs.SetInt(RememberCredentialsKey, 1);
            PlayerPrefs.SetString(NicknameKey, nickname?.Trim() ?? string.Empty);
            PlayerPrefs.SetString(
                PasswordKey,
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(password ?? string.Empty)));
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(RememberCredentialsKey);
            PlayerPrefs.DeleteKey(NicknameKey);
            PlayerPrefs.DeleteKey(PasswordKey);
            PlayerPrefs.Save();
        }
    }
}
