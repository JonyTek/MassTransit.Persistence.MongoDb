using MassTransit.Persistence.MongoDb.Saga;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MassTransit.Persistence.MongoDb.Mappings
{
    public class VersionedSagaConvention : ConventionBase, IClassMapConvention
    {
        public void Apply(BsonClassMap classMap)
        {
            if (classMap.ClassType.IsClass && typeof(IVersionedSaga).IsAssignableFrom(classMap.ClassType))
            {
                classMap.MapIdProperty(nameof(IVersionedSaga.CorrelationId));
            }
        }
    }
}