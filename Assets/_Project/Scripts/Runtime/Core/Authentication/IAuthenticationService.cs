using System.Threading.Tasks;

namespace LootUp.Core.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> TryRestoreSessionAsync();
        Task<AuthenticationResult> SignInAsGuestAsync();
        Task<AuthenticationResult> SignInAsync(string accountId, string password);
        Task SignOutAsync();
    }
}
