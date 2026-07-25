using System.Threading.Tasks;

namespace LootUp.Core.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> TryRestoreSessionAsync();
        Task<NicknameAvailabilityResult> CheckNicknameAvailabilityAsync(string nickname);
        Task<AuthenticationResult> RegisterAsync(
            string accountId,
            string password,
            string nickname);
        Task<AuthenticationResult> RegisterGuestAsync(string nickname, string password);
        Task<AuthenticationResult> SignInGuestAsync(string nickname, string password);
        Task<AuthenticationResult> SignInAsGuestAsync();
        Task<AuthenticationResult> SignInAsync(string accountId, string password);
        Task SignOutAsync();
    }
}
