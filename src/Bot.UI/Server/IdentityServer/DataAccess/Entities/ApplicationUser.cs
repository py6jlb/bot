using Microsoft.AspNetCore.Identity;

namespace Server.IdentityServer.DataAccess.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Sub { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get; set; }
        public string UserData { get; set; }
    }
}