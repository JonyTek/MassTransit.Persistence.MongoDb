using System.Threading.Tasks;
using MassTransit.Pipeline;
using MassTransit.Saga;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MissingPipe<TSaga, TMessage> :
            IPipe<SagaConsumeContext<TSaga, TMessage>>
            where TSaga : class, ISaga
            where TMessage : class
    {
        private readonly IMongoCollection<TSaga> _collection;
        private readonly IPipe<SagaConsumeContext<TSaga, TMessage>> _next;

        public MissingPipe(IMongoCollection<TSaga> collection, IPipe<SagaConsumeContext<TSaga, TMessage>> next)
        {
            _collection = collection;
            _next = next;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _next.Probe(context);
        }

        public async Task Send(SagaConsumeContext<TSaga, TMessage> context)
        {
            SagaConsumeContext<TSaga, TMessage> proxy = new MongoDbSagaConsumeContext<TSaga, TMessage>(_collection, context, context.Saga);

            await _next.Send(proxy).ConfigureAwait(false);

            if (!proxy.IsCompleted)
                await _collection.InsertOneAsync(context.Saga).ConfigureAwait(false);
        }
    }
}