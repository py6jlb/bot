using System.Threading;
using System.Threading.Tasks;
using Bot.WebService.Services.Base;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace Bot.WebService.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly IUpdateService _updateService;
        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update, CancellationToken token)
        {
            await _updateService.HandleUpdate(update, token);
            return Ok();
        }
    }
}