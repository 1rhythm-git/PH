using System;
using System.Threading.Tasks;
using PH.Core.Profile;

namespace PH.Core.Authentication
{
    public static class AuthenticationManager
    {
        private static IAuthenticationService service;
        private static bool isOperationInProgress;

        public static event Action<AuthenticationState> AuthenticationStateChanged;

        public static IAuthenticationService Service => service ??= CreateDefaultService();
        public static AuthenticationState State { get; private set; } = AuthenticationState.SignedOut;
        public static AuthenticationSession CurrentSession { get; private set; }
        public static bool IsAuthenticated => State == AuthenticationState.Authenticated && CurrentSession != null;

        public static void Configure(IAuthenticationService authenticationService)
        {
            service = authenticationService ?? CreateDefaultService();
            CurrentSession = null;
            SetState(AuthenticationState.SignedOut);
        }

        public static async Task<AuthenticationResult> InitializeAsync(bool allowGuestFallback = true)
        {
            if (IsAuthenticated)
            {
                return AuthenticationResult.Success(CurrentSession);
            }

            return await RunAuthenticationOperationAsync(async () =>
            {
                AuthenticationResult result = await Service.TryRestoreSessionAsync();
                if (!result.Succeeded
                    && allowGuestFallback
                    && result.Failure == AuthenticationFailure.NoSavedSession)
                {
                    result = await Service.SignInAsGuestAsync();
                }

                return result;
            });
        }

        public static Task<AuthenticationResult> SignInAsGuestAsync()
        {
            return RunAuthenticationOperationAsync(() => Service.SignInAsGuestAsync());
        }

        public static Task<AuthenticationResult> SignInAsync(string accountId, string password)
        {
            return RunAuthenticationOperationAsync(() => Service.SignInAsync(accountId, password));
        }

        public static async Task SignOutAsync()
        {
            if (isOperationInProgress)
            {
                return;
            }

            isOperationInProgress = true;
            try
            {
                await Service.SignOutAsync();
                CurrentSession = null;
                SetState(AuthenticationState.SignedOut);
            }
            catch
            {
                SetState(AuthenticationState.Failed);
            }
            finally
            {
                isOperationInProgress = false;
            }
        }

        private static async Task<AuthenticationResult> RunAuthenticationOperationAsync(
            Func<Task<AuthenticationResult>> operation)
        {
            if (isOperationInProgress)
            {
                return AuthenticationResult.Fail(
                    AuthenticationFailure.OperationInProgress,
                    "Another authentication operation is already running.");
            }

            isOperationInProgress = true;
            SetState(AuthenticationState.Authenticating);

            AuthenticationResult result;
            try
            {
                result = await operation();
            }
            catch (Exception exception)
            {
                result = AuthenticationResult.Fail(AuthenticationFailure.Unexpected, exception.Message);
            }
            finally
            {
                isOperationInProgress = false;
            }

            if (result.Succeeded)
            {
                CurrentSession = result.Session;
                UserProfileManager.SetIdentity(result.Session.UserId, result.Session.Nickname);
                SetState(AuthenticationState.Authenticated);
            }
            else
            {
                CurrentSession = null;
                SetState(AuthenticationState.Failed);
            }

            return result;
        }

        private static IAuthenticationService CreateDefaultService()
        {
            return new LocalAuthenticationService(UserProfileManager.UserId, UserProfileManager.Nickname);
        }

        private static void SetState(AuthenticationState state)
        {
            State = state;
            AuthenticationStateChanged?.Invoke(state);
        }
    }
}
