using MassTransit.Saga;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public interface IMongoDbSagaConsumeContextFactory
    {
        SagaConsumeContext<TSaga, TMessage> Create<TSaga, TMessage>(IMongoCollection<TSaga> collection, ConsumeContext<TMessage> message, TSaga instance, bool existing = true) 
            where TSaga : class, ISaga 
            where TMessage : class;
    }
}