using Bot.DataAccessLayer.Entities;
using LiteDB;

namespace Bot.DataAccessLayer.Abstraction
{
    public interface ILiteDbContext
    {
        LiteDatabase Context { get; }
        BsonMapper Mapper { get; }
        ILiteCollection<MoneyTransaction> MoneyTransactions { get; }
    }
}