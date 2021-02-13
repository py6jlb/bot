using Server.IdentityServer.Accounts.Models;

namespace Server.IdentityServer.Accounts.ViewModels
{
    public class LogoutViewModel: LogoutInputModel
    {
        public bool ShowLogoutPrompt { get; set; } = true;
    }
}