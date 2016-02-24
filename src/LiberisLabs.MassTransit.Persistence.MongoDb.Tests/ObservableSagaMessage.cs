using System;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests
{
    [Serializable]
    public class ObservableSagaMessage
    {
        public string Name { get; set; }
    }
}