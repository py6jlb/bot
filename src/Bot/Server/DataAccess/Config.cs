using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace Bot.Server.DataAccess
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API")
            };

        public static IEnumerable<IdentityServer4.Models.Client> Clients =>
            new List<IdentityServer4.Models.Client>
            {
                // machine to machine client
                new IdentityServer4.Models.Client
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { "api1" }
                },
                
                new IdentityServer4.Models.Client
                {
                    ClientId = "blazor_app",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    }
                }
            };
    }
}