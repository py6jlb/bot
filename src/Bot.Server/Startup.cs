using System.IO;
using Bot.Server.IdentityServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bot.Server
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly bool _behindReverseProxy;
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
            _ = bool.TryParse(_config["USE_REVERSE_PROXY"], out _behindReverseProxy);
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityDb(_config)
                .AddAspNetIdentity()
                .AddConfiguredIdentityServer(_config);
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddViewLocations();
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"..\..\keys\wasm_server\"))
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
            services.AddCors();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.InitializeIdentityServerDatabase();
            if (_behindReverseProxy)
            {
                var fordwardedHeaderOptions = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };
                fordwardedHeaderOptions.KnownNetworks.Clear();
                fordwardedHeaderOptions.KnownProxies.Clear();

                app.UseForwardedHeaders(fordwardedHeaderOptions);

                //var subdirPath = _config["SUBDIR"];
                //if (!string.IsNullOrWhiteSpace(subdirPath)) app.UsePathBase(new PathString(subdirPath));
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
                app.SeedDevelopUsersData();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}