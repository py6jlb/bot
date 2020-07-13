using System.Collections.Generic;
using System.Linq;
using Bot.DataAccessLayer.Abstraction;
using Bot.DataAccessLayer.Entities;
using LiteDB;

namespace Bot.DataAccessLayer.Services
{
    public class MoneyTransactionRepository : IMoneyTransactionRepository
    {
        private ILiteCollection<MoneyTransaction> _collection;
        public MoneyTransactionRepository(ILiteDbContext liteDbContext)
        {
            _collection = liteDbContext.MoneyTransactions;
        }

        public int Delete(ObjectId id)
        {
            return _collection.DeleteMany(x => x.Id == id);
        }

        public IEnumerable<MoneyTransaction> FindAll()
        {
            var result = _collection.FindAll();
            return result;
        }

        public MoneyTransaction FindOne(ObjectId id)
        {
            return _collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public ObjectId Insert(MoneyTransaction moneyTransaction)
        {
            return _collection.Insert(moneyTransaction);
        }

        public bool Update(MoneyTransaction moneyTransaction)
        {
            return _collection.Update(moneyTransaction);
        }
    }
}