using System;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    public class InitiateSimpleSaga : SimpleSagaMessageBase
    {
        public InitiateSimpleSaga()
        {
        }

        public InitiateSimpleSaga(Guid correlationId)
            : base(correlationId)
        {
        }

        public string Name { get; set; }
    }
}