using Bot.Server.IdentityServer.Account.Models;

namespace Bot.Server.IdentityServer.Account.ViewModels
{
    public class LogoutViewModel: LogoutInputModel
    {
        public bool ShowLogoutPrompt { get; set; } = true;
    }
}