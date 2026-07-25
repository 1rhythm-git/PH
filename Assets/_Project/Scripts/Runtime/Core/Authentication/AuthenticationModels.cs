using System;

namespace LootUp.Core.Authentication
{
    public enum AuthenticationProvider
    {
        LocalGuest,
        Google,
        Backnd
    }

    public enum AuthenticationState
    {
        SignedOut,
        Authenticating,
        Authenticated,
        Failed
    }

    public enum AuthenticationFailure
    {
        None,
        NoSavedSession,
        InvalidAccountId,
        InvalidCredentials,
        AccountAlreadyExists,
        InvalidNickname,
        NicknameAlreadyExists,
        NicknameNotFound,
        GuestAccountLimitReached,
        WeakPassword,
        NetworkUnavailable,
        ProviderUnavailable,
        OperationInProgress,
        Unexpected
    }

    public sealed class AuthenticationSession
    {
        public AuthenticationSession(
            string userId,
            string nickname,
            AuthenticationProvider provider,
            bool isGuest,
            string accountId = null)
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? string.Empty : userId.Trim();
            Nickname = string.IsNullOrWhiteSpace(nickname) ? "Player" : nickname.Trim();
            Provider = provider;
            IsGuest = isGuest;
            AccountId = string.IsNullOrWhiteSpace(accountId)
                ? string.Empty
                : accountId.Trim();
        }

        public string UserId { get; }
        public string Nickname { get; }
        public AuthenticationProvider Provider { get; }
        public bool IsGuest { get; }
        public string AccountId { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(UserId);
    }

    public readonly struct AuthenticationResult
    {
        private AuthenticationResult(
            bool succeeded,
            AuthenticationSession session,
            AuthenticationFailure failure,
            string message)
        {
            Succeeded = succeeded;
            Session = session;
            Failure = failure;
            Message = message ?? string.Empty;
        }

        public bool Succeeded { get; }
        public AuthenticationSession Session { get; }
        public AuthenticationFailure Failure { get; }
        public string Message { get; }

        public static AuthenticationResult Success(AuthenticationSession session)
        {
            if (session == null || !session.IsValid)
            {
                return Fail(
                    AuthenticationFailure.Unexpected,
                    "Authentication provider returned an invalid session.");
            }

            return new AuthenticationResult(
                true,
                session,
                AuthenticationFailure.None,
                string.Empty);
        }

        public static AuthenticationResult Fail(
            AuthenticationFailure failure,
            string message)
        {
            return new AuthenticationResult(false, null, failure, message);
        }
    }

    public readonly struct NicknameAvailabilityResult
    {
        private NicknameAvailabilityResult(
            bool isAvailable,
            AuthenticationFailure failure,
            string message)
        {
            IsAvailable = isAvailable;
            Failure = failure;
            Message = message ?? string.Empty;
        }

        public bool IsAvailable { get; }
        public AuthenticationFailure Failure { get; }
        public string Message { get; }

        public static NicknameAvailabilityResult Available()
        {
            return new NicknameAvailabilityResult(
                true,
                AuthenticationFailure.None,
                string.Empty);
        }

        public static NicknameAvailabilityResult Unavailable(
            AuthenticationFailure failure,
            string message)
        {
            return new NicknameAvailabilityResult(false, failure, message);
        }
    }
}
