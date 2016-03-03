using MassTransit.Persistence.MongoDb.Saga;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MassTransit.Persistence.MongoDb.Mappings
{
    public class MappingConfigurator
    {
        public void Configure()
        {
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreIfNullConvention(true),
                new IgnoreExtraElementsConvention(true),
                new BullshitConvention()
            };

            ConventionRegistry.Register("LiberisLabs.MassTransit.Persistence.MongoDb Conventions", 
                conventionPack, 
                t => t.IsClass &&
                typeof(IVersionedSaga).IsAssignableFrom(t));


        }
    }

    public class BullshitConvention : ConventionBase, IClassMapConvention
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