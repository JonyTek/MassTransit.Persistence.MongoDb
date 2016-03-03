using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public interface IMongoDbSagaConsumeContextFactory
    {
        SagaConsumeContext<TSaga, TMessage> Create<TSaga, TMessage>(IMongoCollection<TSaga> collection, ConsumeContext<TMessage> message, TSaga instance, bool existing = true) 
            where TSaga : class, IVersionedSaga 
            where TMessage : class;
    }
}