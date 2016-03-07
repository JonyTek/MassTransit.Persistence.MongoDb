using MassTransit.Persistence.MongoDb.Mappings;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    [SetUpFixture]
    public class Initialise
    {
        [OneTimeSetUp]
        public void Start()
        {
            new MappingConfigurator().Configure();
        }
    }
}