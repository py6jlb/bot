using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Bot.Server.IdentityServer.DataAccess.Contexts;
using Bot.Server.IdentityServer.DataAccess.Entities;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Server.IdentityServer.DataAccess
{
    public static class DataInitializer
    {
        public static void InitializeIdentityServerDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            serviceScope?.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
            var context = serviceScope?.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            context?.Database.Migrate();
            if (context == null) return;
            
            if (!context.Clients.Any())
            {
                var clients = config.GetSection("Clients").Get<IdentityServer4.Models.Client[]>();
                foreach (var client in clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                var resources = new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email()
                };
                var additionalResources = config.GetSection("Resources").Get<IdentityResource[]>();
                if (additionalResources.Any())
                {
                    resources.AddRange(additionalResources);
                }
                foreach (var resource in resources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                var apiScopes = config.GetSection("ApiScopes").GetChildren();
                foreach (var apiScope in apiScopes)
                {
                    var name = apiScope.GetSection("Name").Get<string>();
                    var displayName =  apiScope.GetSection("DisplayName").Get<string>();
                    var userClaims = apiScope.GetSection("UserClaims").Get<IEnumerable<string>>();
                    var res = new ApiScope(name, displayName, userClaims);
                    context.ApiScopes.Add(res.ToEntity());
                }
                context.SaveChanges();
            }
        }

        public static void EnsureSeedDevelopUsersData(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            var context = serviceScope?.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context?.Database.Migrate();
            var userMgr = serviceScope?.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceScope?.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if(userMgr is null || roleManager is null) return;
            
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@email.com",
                    EmailConfirmed = true,
                };
                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                
            }
            
            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@email.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                    new Claim("location", "somewhere")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }
    }
}