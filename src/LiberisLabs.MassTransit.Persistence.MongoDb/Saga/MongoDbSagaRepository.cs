using System.Threading.Tasks;
using MassTransit.Pipeline;
using MassTransit.Saga;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbSagaRepository<TSaga> : ISagaRepository<TSaga> where TSaga : class, ISaga
    {
        private readonly IMongoCollection<TSaga> _collection;

        public MongoDbSagaRepository(string connectionString, string database)
            : this(new MongoClient(connectionString).GetDatabase(database))
        {
        }

        public MongoDbSagaRepository(MongoUrl mongoUrl)
            : this(mongoUrl.Url, mongoUrl.DatabaseName)
        {
        }

        public MongoDbSagaRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<TSaga>("sagas");
        }

        public void Probe(ProbeContext context)
        {
            throw new System.NotImplementedException();
        }

        public async Task Send<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            if (!context.CorrelationId.HasValue)
                throw new SagaException("The CorrelationId was not specified", typeof(TSaga), typeof(T));

            TSaga instance;

            if (policy.PreInsertInstance(context, out instance))
            {
                await _collection.InsertOneAsync(instance, new InsertOneOptions(), context.CancellationToken).ConfigureAwait(false);
            }

            if (instance == null)
            {
                instance = await _collection.Find(x => x.CorrelationId == context.CorrelationId).SingleAsync(context.CancellationToken).ConfigureAwait(false);
            }

            var sagaConsumeContext = new MongoDbSagaConsumeContext<TSaga,T>(_collection, context, instance);

            await policy.Existing(sagaConsumeContext, next).ConfigureAwait(false);
        }

        public Task SendQuery<T>(SagaQueryConsumeContext<TSaga, T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task Find(ISagaQuery<TSaga> query)
        {
            throw new System.NotImplementedException();
        }
    }
}