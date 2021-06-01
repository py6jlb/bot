using System;
using Newtonsoft.Json;

namespace Bot.WebService.Services.Dto
{
    public class BaseInfo
    {
        public string Id { get; set; }
        public float Number { get; set; }
        public string FromUserName { get; set; }
        public DateTime RegisterDate { get; set; }

        public string Sign { get; set; }

        public override string ToString()
        {
            var str = JsonConvert.SerializeObject(this);
            return str;
        }
    }
}
