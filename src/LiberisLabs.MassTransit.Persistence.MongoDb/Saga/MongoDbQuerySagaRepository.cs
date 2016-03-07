using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit.Saga;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbQuerySagaRepository<TSaga> : IQuerySagaRepository<TSaga> where TSaga : class, ISaga
    {
        private readonly IMongoCollection<TSaga> _collection;

        public MongoDbQuerySagaRepository(string connectionString, string database)
            : this(new MongoClient(connectionString).GetDatabase(database))
        {
        }

        public MongoDbQuerySagaRepository(MongoUrl mongoUrl)
            : this(mongoUrl.Url, mongoUrl.DatabaseName)
        {
        }

        public MongoDbQuerySagaRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<TSaga>("sagas");
        }

        public async Task<IEnumerable<Guid>> Find(ISagaQuery<TSaga> query)
        {
            return await _collection.Find(query.FilterExpression)
                                     .Project(x => x.CorrelationId)
                                     .ToListAsync()
                                     .ConfigureAwait(false);
        }
    }
}