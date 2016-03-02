using MassTransit.Saga;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public interface IVersionedSaga : ISaga
    {
        int Version { get; set; }
    }
}