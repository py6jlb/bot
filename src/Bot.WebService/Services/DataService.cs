using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bot.WebService.Services.Base;
using Bot.WebService.Services.Dto;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Protos;

namespace Bot.WebService.Services
{
    public class DataService : IDataService
    {
        private readonly string _endpoint;
        private readonly HttpClientHandler _httpHandler;

        public DataService(IConfiguration config)
        {
            _endpoint = config["BOT_DATA_STORE_ENDPOINT"] ?? throw new ArgumentNullException("botDatastoreEndpoint",
                "Параметр не может быть null, проверьте перменные окружения.");
            _httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        }

        public async Task<BaseInfo> SaveBaseData(BaseInfo data)
        {
            var requestData = new SaveMoneyTransactionRequest
            {
                RegisterDate = data.RegisterDate.ToUniversalTime().Subtract(
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                ).TotalMilliseconds,
                Sum = data.Number,
                Sign = data.Sign,
                FromUserName = data.FromUserName
            };
            using var channel = GrpcChannel.ForAddress(_endpoint, new GrpcChannelOptions { HttpHandler = _httpHandler });
            var client = new Writer.WriterClient(channel);
            var response = await client.SaveMoneyTransactionAsync(requestData);
            data.Id = response.Id;
            return data;
        }

        public async Task<bool> SetCategory(string id, CategoryInfo data)
        {
            var requestData = new SetCategoryRequest
            {
                Id = id,
                CategoryHumanName = data.CategoryHumanName,
                CategoryIcon = data.Icon,
                CategoryName = data.CategoryName
            };

            using var channel = GrpcChannel.ForAddress(_endpoint, new GrpcChannelOptions { HttpHandler = _httpHandler });
            var client = new Writer.WriterClient(channel);
            var response = await client.SetCategoryAsync(requestData);
            return response.Success;
        }
    }
}
