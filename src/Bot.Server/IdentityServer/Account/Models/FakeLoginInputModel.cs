using System.ComponentModel.DataAnnotations;

namespace Bot.Server.IdentityServer.Account.Models
{
    public class FakeLoginInputModel
    {
        [Required]
        [Display(Name="Пользователь")]
        public string UserName { get; set; }
        public string ReturnUrl { get; set; }
        
    }
}