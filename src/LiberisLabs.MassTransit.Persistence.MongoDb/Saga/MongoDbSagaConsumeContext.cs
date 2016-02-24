using System.Threading.Tasks;
using MassTransit.Context;
using MassTransit.Saga;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbSagaConsumeContext<TSaga, TMessage> : 
        ConsumeContextProxyScope<TMessage>,
        SagaConsumeContext<TSaga, TMessage>
        where TMessage : class
        where TSaga : class, ISaga
    {
        private IMongoCollection<TSaga> _collection;
        private bool _existing;

        public MongoDbSagaConsumeContext(IMongoCollection<TSaga> collection, ConsumeContext<TMessage> context, TSaga instance, bool existing = true) : base(context)
        {
            Saga = instance;
            _collection = collection;
            _existing = existing;
        }

        public SagaConsumeContext<TSaga, T> PopContext<T>() where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task SetCompleted()
        {
            throw new System.NotImplementedException();
        }

        public TSaga Saga { get; }

        public bool IsCompleted { get; }
    }
}