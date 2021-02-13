using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.IdentityServer.DataAccess;
using Server.IdentityServer.DataAccess.Contexts;
using Server.IdentityServer.DataAccess.Entities;
using Server.IdentityServer.Services;

namespace Server.IdentityServer.Extensions
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
                }).AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            return services;
        }
        
        public static IServiceCollection AddConfiguredIdentityServer(this IServiceCollection services,
            IConfiguration config)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            var builder = services.AddIdentityServer(options =>
                {
                    options.Discovery.CustomEntries.Add("additional_data", "~/AdditionalData");
                })
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
            services.AddLocalApiAuthentication();
            return services;
        }
        
        // public static IApplicationBuilder SeedDevelopUsersData(this IApplicationBuilder app)
        // {
        //     DataInitializer.EnsureSeedDevelopUsersData(app);
        //     return app;
        // }
        
        public static IApplicationBuilder InitializeIdentityServerDatabase(this IApplicationBuilder app)
        {
            DataInitializer.InitializeIdentityServerDatabase(app);
            return app;
        }
    }
}