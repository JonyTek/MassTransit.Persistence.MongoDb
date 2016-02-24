using System;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    [Serializable]
    public class ObservableSagaMessage
    {
        public string Name { get; set; }
    }
}