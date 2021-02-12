using Newtonsoft.Json;
using System;

namespace Bot.TelegramWorker.Services.Dto
{
    public class CallbackInfo
    {
        public string Id { get; set; }
        public string Ctg { get; set; }

        public override string ToString()
        {
            var str = JsonConvert.SerializeObject(this);
            return str;
        }
    }
}
