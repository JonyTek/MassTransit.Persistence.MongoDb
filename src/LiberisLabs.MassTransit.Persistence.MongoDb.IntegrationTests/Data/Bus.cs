using System.Threading.Tasks;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MongoDB.Driver;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Data
{
    public class Bus
    {
        public static async Task<IBusControl> StartAsync()
        {
            var busControl = global::MassTransit.Bus.Factory.CreateUsingInMemory(configurator =>
            {
                configurator.ReceiveEndpoint("input_queue",
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Saga(new MongoDbSagaRepository<SimpleSaga>(new MongoUrl("mongodb://localhost/sagaTest")));
                    });
            });

            await busControl.StartAsync();

            return busControl;
        }
    }
}