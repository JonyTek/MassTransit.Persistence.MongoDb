using MassTransit.Persistence.MongoDb.Saga;
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
                new VersionedSagaConvention()
            };

            ConventionRegistry.Register("LiberisLabs.MassTransit.Persistence.MongoDb Conventions",
                conventionPack,
                t => t.IsClass &&
                typeof(IVersionedSaga).IsAssignableFrom(t));
        }
    }
}