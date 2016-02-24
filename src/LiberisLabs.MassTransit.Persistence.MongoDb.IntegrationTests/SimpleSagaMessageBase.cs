using System;
using MassTransit;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    public class SimpleSagaMessageBase :
        CorrelatedBy<Guid>
    {
        public SimpleSagaMessageBase()
        {
        }

        public SimpleSagaMessageBase(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; set; }
    }
}