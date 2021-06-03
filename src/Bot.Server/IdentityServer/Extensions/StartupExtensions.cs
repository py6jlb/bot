using System.Reflection;
using Bot.Server.IdentityServer.DataAccess;
using Bot.Server.IdentityServer.DataAccess.Contexts;
using Bot.Server.IdentityServer.DataAccess.Entities;
using Bot.Server.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Server.IdentityServer.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddIdentityDb(this IServiceCollection services, IConfiguration config)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseSqlite(config.GetConnectionString("DefaultConnection"), sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly)));
            return services;
        }
        
        public static IServiceCollection AddAspNetIdentity(this IServiceCollection services)
        {

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                }).AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddAuthentication().AddIdentityServerJwt();
            return services;
        }
        
        public static IServiceCollection AddConfiguredIdentityServer(this IServiceCollection services,
            IConfiguration config)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            var builder = services.AddIdentityServer()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlite(
                        config.GetConnectionString("DefaultConnection"), sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly));
                }).AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlite(
                        config.GetConnectionString("DefaultConnection"), sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly));
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<IdentityProfileService>();
                
            builder.AddDeveloperSigningCredential();
            //services.AddLocalApiAuthentication();
            return services;
        }
        
        public static IApplicationBuilder SeedDevelopUsersData(this IApplicationBuilder app)
        {
            DataInitializer.EnsureSeedDevelopUsersData(app);
            return app;
        }
        
        public static IApplicationBuilder InitializeIdentityServerDatabase(this IApplicationBuilder app)
        {
            DataInitializer.InitializeIdentityServerDatabase(app);
            return app;
        }

        public static IServiceCollection AddViewLocations(this IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(options => {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
            return services;
        }
    }
}