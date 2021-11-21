using System;
using System.Reflection;
using Bot.Server.DataAccess;
using Bot.Server.DataAccess.Contexts;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bot.Server
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<AppIdentityDbContext>(opts =>
                opts.UseSqlite(_config.GetConnectionString("DefaultConnection"), sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly)));
            
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

            services.AddIdentityServer(options =>
                {
                    options.IssuerUri = "bot_identity_server";
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.UserInteraction.LoginUrl = "Identity/Account/Login";
                    options.UserInteraction.LogoutUrl = "Identity/Account/Logout";
                    options.UserInteraction.ConsentUrl = "Identity/Consent";
                    options.UserInteraction.DeviceVerificationUrl = "Identity/Device";
                    options.Authentication = new AuthenticationOptions()
                    {
                        CookieLifetime = TimeSpan.FromHours(10), // ID server cookie timeout set to 10 hours
                        CookieSlidingExpiration = true
                    };
                }).AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<IdentityUser>()
                .AddDeveloperSigningCredential();

            services.AddAuthentication();
            services.AddAuthorization();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.EnsureSeedData();
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseIdentityServer();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapAreaControllerRoute("Identity", "Identity", "Identity/{controller}/{action=Index}/{id?}");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
