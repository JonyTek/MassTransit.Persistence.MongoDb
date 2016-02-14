using System.Threading.Tasks;
using MassTransit.Context;
using MassTransit.Saga;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbSagaConsumeContext<TSaga, TMessage> : 
        ConsumeContextProxyScope<TMessage>,
        SagaConsumeContext<TSaga, TMessage>
        where TMessage : class
        where TSaga : class, ISaga
    {
        public MongoDbSagaConsumeContext(ConsumeContext<TMessage> context) : base(context)
        {
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