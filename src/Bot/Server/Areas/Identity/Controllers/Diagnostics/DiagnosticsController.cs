// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using System.Threading.Tasks;
using Bot.Server.Areas.Identity.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Bot.Server.Areas.Identity.Controllers.Diagnostics
{
    [Area("Identity")]
    [SecurityHeaders]
    [Authorize]
    public class DiagnosticsController : Controller
    {
        private readonly IHostEnvironment _env;

        public DiagnosticsController(IHostEnvironment env)
        {
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            //var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };
            //if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
            //{
            //    return NotFound();
            //}

            if (!_env.IsDevelopment())
            {
                return NotFound();
            }

            var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());
            return View(model);
        }
    }
}