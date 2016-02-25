using System;
using MassTransit;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    [Serializable]
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