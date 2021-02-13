using System.ComponentModel.DataAnnotations;

namespace Server.IdentityServer.Account.Models
{
    public class LoginInputModel
    {
        [Required]
        [Display(Name="Имя пользователя")]
        public string Username { get; set; }
        [Required]
        [Display(Name="Пароль")]
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}