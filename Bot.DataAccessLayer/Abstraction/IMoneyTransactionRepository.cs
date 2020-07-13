using System.Collections.Generic;
using Bot.DataAccessLayer.Entities;
using LiteDB;

namespace Bot.DataAccessLayer.Abstraction
{
    public interface IMoneyTransactionRepository
    {
        int Delete(ObjectId id);
        IEnumerable<MoneyTransaction> FindAll();
        MoneyTransaction FindOne(ObjectId id);
        ObjectId Insert(MoneyTransaction forecast);
        bool Update(MoneyTransaction forecast);
    }
}