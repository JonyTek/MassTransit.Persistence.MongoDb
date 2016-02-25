using System;
using System.Threading.Tasks;
using MassTransit.Context;
using MassTransit.Saga;
using MassTransit.Util;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbSagaConsumeContext<TSaga, TMessage> : 
        ConsumeContextProxyScope<TMessage>,
        SagaConsumeContext<TSaga, TMessage>
        where TMessage : class
        where TSaga : class, ISaga
    {
        private readonly IMongoCollection<TSaga> _collection;
        private readonly bool _existing;

        public MongoDbSagaConsumeContext(IMongoCollection<TSaga> collection, ConsumeContext<TMessage> context, TSaga instance, bool existing = true) 
            : base(context)
        {
            Saga = instance;
            _collection = collection;
            _existing = existing;
        }

        Guid? MessageContext.CorrelationId => Saga.CorrelationId;

        public SagaConsumeContext<TSaga, T> PopContext<T>() where T : class
        {
            var context = this as SagaConsumeContext<TSaga, T>;
            if (context == null)
                throw new ContextException($"The ConsumeContext<{TypeMetadataCache<TMessage>.ShortName}> could not be cast to {TypeMetadataCache<T>.ShortName}");

            return context;
        }

        public async Task SetCompleted()
        {
            IsCompleted = true;

            if (_existing)
            {
                await _collection.DeleteOneAsync(x => x.CorrelationId == Saga.CorrelationId, CancellationToken).ConfigureAwait(false);
            }

        }

        public TSaga Saga { get; }

        public bool IsCompleted { get; private set; }
    }
}