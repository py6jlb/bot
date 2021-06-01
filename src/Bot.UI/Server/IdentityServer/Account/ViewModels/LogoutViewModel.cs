using Server.IdentityServer.Account.Models;

namespace Server.IdentityServer.Account.ViewModels
{
    public class LogoutViewModel: LogoutInputModel
    {
        public bool ShowLogoutPrompt { get; set; } = true;
    }
}