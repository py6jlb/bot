using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Bot.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app"); builder.Services.AddHttpClient("ServerAPI",
                    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));
            //builder.Services.AddOidcAuthentication(options => builder.Configuration.Bind("oidc", options.ProviderOptions));
            builder.Services.AddOidcAuthentication(opt =>
            {
                opt.ProviderOptions.Authority = "https://localhost";
                opt.ProviderOptions.ClientId = "blazor_app";
                opt.ProviderOptions.ResponseType = "code";
                opt.ProviderOptions.PostLogoutRedirectUri = "https://localhost/authentication/logout-callback";
                opt.ProviderOptions.RedirectUri = "https://localhost/authentication/login-callback";
            });
            await builder.Build().RunAsync();
        }
    }
}
