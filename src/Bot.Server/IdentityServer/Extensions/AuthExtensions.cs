using System;
using Bot.Server.IdentityServer.Extensions.Models;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bot.Server.IdentityServer.Extensions
{
    public static class AuthExtensions
    {
        public static bool IsNativeClient(this AuthorizationRequest context)
        {
            return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
                   && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
        }

        public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
        {
            controller.HttpContext.Response.StatusCode = 200;
            controller.HttpContext.Response.Headers["Location"] = "";
            return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
        }
    }
}