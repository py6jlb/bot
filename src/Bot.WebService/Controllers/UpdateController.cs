using System.Threading;
using System.Threading.Tasks;
using Bot.WebService.Services.Base;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace Bot.WebService.Controllers
{
    [Route("[controller]")]
    public class UpdateController : Controller
    {
        private readonly IUpdateService _updateService;
        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }


        [HttpGet]
        public IActionResult Get(CancellationToken token)
        {
            return Ok("szdgsdfgsdfgsdfgsdfgsdfgsdfgsdfg");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update, CancellationToken token)
        {
            await _updateService.HandleUpdate(update, token);
            return Ok();
        }
    }
}