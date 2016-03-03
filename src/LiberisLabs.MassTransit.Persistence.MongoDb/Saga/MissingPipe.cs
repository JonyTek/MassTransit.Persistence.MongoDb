using System.Threading.Tasks;
using MassTransit.Logging;
using MassTransit.Pipeline;
using MassTransit.Util;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MissingPipe<TSaga, TMessage> :
            IPipe<SagaConsumeContext<TSaga, TMessage>>
            where TSaga : class, IVersionedSaga
            where TMessage : class
    {
        private static readonly ILog _log = Logger.Get<MongoDbSagaRepository<TSaga>>();
        private readonly IMongoCollection<TSaga> _collection;
        private readonly IPipe<SagaConsumeContext<TSaga, TMessage>> _next;
        private readonly IMongoDbSagaConsumeContextFactory _mongoDbSagaConsumeContextFactory;

        public MissingPipe(IMongoCollection<TSaga> collection, IPipe<SagaConsumeContext<TSaga, TMessage>> next, IMongoDbSagaConsumeContextFactory mongoDbSagaConsumeContextFactory)
        {
            _collection = collection;
            _next = next;
            _mongoDbSagaConsumeContextFactory = mongoDbSagaConsumeContextFactory;
        }

        public void Probe(ProbeContext context)
        {
            _next.Probe(context);
        }

        public async Task Send(SagaConsumeContext<TSaga, TMessage> context)
        {
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("SAGA:{0}:{1} Added {2}", TypeMetadataCache<TSaga>.ShortName, TypeMetadataCache<TMessage>.ShortName);
            }

            SagaConsumeContext<TSaga, TMessage> proxy = _mongoDbSagaConsumeContextFactory.Create(_collection, context, context.Saga, false);

            await _next.Send(proxy).ConfigureAwait(false);

            if (!proxy.IsCompleted)
                await _collection.InsertOneAsync(context.Saga).ConfigureAwait(false);
        }
    }
}