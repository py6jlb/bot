using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Server.Error.Models;

namespace Server.Error
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            var model = new ErrorModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View("~/Error/Views/Error.cshtml", model);
        }
    }
}