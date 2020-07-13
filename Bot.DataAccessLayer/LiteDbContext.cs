using System;
using Bot.DataAccessLayer.Abstraction;
using Bot.DataAccessLayer.Entities;
using Bot.DataAccessLayer.Options;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Bot.DataAccessLayer
{
    public class LiteDbContext : ILiteDbContext
    {
        public LiteDatabase Context { get; }
        public BsonMapper Mapper { get; }
        public ILiteCollection<MoneyTransaction> MoneyTransactions { get; }

        public LiteDbContext(IOptions<LiteDbOptions> options)
        {
            try
            {
                Mapper = GetMapper();
                Context = new LiteDatabase(options.Value.DatabaseLocation, Mapper);
                MoneyTransactions = Context.GetCollection<MoneyTransaction>("MoneyTransactions");
            }
            catch (Exception ex)
            {
                throw new Exception("Неудалось создать или подключиться к файлу базы данных", ex);
            }
        }

        private BsonMapper GetMapper()
        {
            var mapper = BsonMapper.Global;
            mapper.Entity<MoneyTransaction>().Id(x => x.Id);
            return mapper;
        }
    }
}