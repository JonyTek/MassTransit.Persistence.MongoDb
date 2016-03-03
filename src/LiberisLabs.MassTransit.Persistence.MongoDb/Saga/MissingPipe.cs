using System.Threading.Tasks;
using MassTransit.Pipeline;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MissingPipe<TSaga, TMessage> :
            IPipe<SagaConsumeContext<TSaga, TMessage>>
            where TSaga : class, IVersionedSaga
            where TMessage : class
    {
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
            SagaConsumeContext<TSaga, TMessage> proxy = _mongoDbSagaConsumeContextFactory.Create(_collection, context, context.Saga, false);

            await _next.Send(proxy).ConfigureAwait(false);

            if (!proxy.IsCompleted)
                await _collection.InsertOneAsync(context.Saga).ConfigureAwait(false);

            //await _collection.FindOneAndReplaceAsync(x => x.CorrelationId == context.Saga.CorrelationId && x.Version < context.Saga.Version, context.Saga).ConfigureAwait(false);
        }
    }
}