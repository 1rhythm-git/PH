using System;

namespace LootUp.Core.Authentication
{
    public enum AuthenticationProvider
    {
        LocalGuest,
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
        InvalidCredentials,
        NetworkUnavailable,
        ProviderUnavailable,
        OperationInProgress,
        Unexpected
    }

    public sealed class AuthenticationSession
    {
        public AuthenticationSession(string userId, string nickname, AuthenticationProvider provider, bool isGuest)
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? string.Empty : userId.Trim();
            Nickname = string.IsNullOrWhiteSpace(nickname) ? "Player" : nickname.Trim();
            Provider = provider;
            IsGuest = isGuest;
        }

        public string UserId { get; }
        public string Nickname { get; }
        public AuthenticationProvider Provider { get; }
        public bool IsGuest { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(UserId);
    }

    public readonly struct AuthenticationResult
    {
        private AuthenticationResult(bool succeeded, AuthenticationSession session, AuthenticationFailure failure, string message)
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
                return Fail(AuthenticationFailure.Unexpected, "Authentication provider returned an invalid session.");
            }

            return new AuthenticationResult(true, session, AuthenticationFailure.None, string.Empty);
        }

        public static AuthenticationResult Fail(AuthenticationFailure failure, string message)
        {
            return new AuthenticationResult(false, null, failure, message);
        }
    }
}
