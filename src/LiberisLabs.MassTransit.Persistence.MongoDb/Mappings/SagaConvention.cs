using MassTransit.Saga;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MassTransit.Persistence.MongoDb.Mappings
{
    public class SagaConvention : ConventionBase, IClassMapConvention
    {
        public void Apply(BsonClassMap classMap)
        {
            if (classMap.ClassType.IsClass && typeof(ISaga).IsAssignableFrom(classMap.ClassType))
            {
                classMap.MapIdProperty(nameof(ISaga.CorrelationId));
            }
        }
    }
}