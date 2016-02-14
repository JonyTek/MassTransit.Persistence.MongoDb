using System;
using MassTransit;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    [TestFixture]
    public class LocatingAnExistingSagaTests
    {
        private Guid _sagaId;

        [OneTimeSetUp]
        public void GivenASimpleSaga()
        {
            _sagaId = Guid.NewGuid();
            var message = new SimpleSagaMessage(_sagaId);


        }
    }

    public class SimpleSagaMessage : CorrelatedBy<Guid>
    {
        public SimpleSagaMessage(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
    }
}
