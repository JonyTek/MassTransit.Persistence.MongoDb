using System;
using MassTransit;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    public class InitiateSimpleSaga : CorrelatedBy<Guid>
    {
        public InitiateSimpleSaga()
        {
        }

        public InitiateSimpleSaga(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }

        public string Name { get; set; }
    }
}