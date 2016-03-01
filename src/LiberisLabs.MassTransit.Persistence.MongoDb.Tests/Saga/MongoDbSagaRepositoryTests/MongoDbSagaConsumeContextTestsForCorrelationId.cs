using System;
using LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data;
using MassTransit.Persistence.MongoDb.Saga;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MongoDbSagaRepositoryTests
{
    [TestFixture]
    public class MongoDbSagaConsumeContextTestsForCorrelationId
    {
        private Guid _correlationId;
        private MongoDbSagaConsumeContext<SimpleSaga, InitiateSimpleSaga> _context;

        [OneTimeSetUp]
        public void GivenAMongoDbSagaConsumeContext_WhenGettingCorrelationId()
        {
            _correlationId = Guid.NewGuid();
            var saga = new SimpleSaga {CorrelationId = _correlationId};

            _context = new MongoDbSagaConsumeContext<SimpleSaga, InitiateSimpleSaga>(null, null, saga);
        }

        [Test]
        public void ThenInstancesCorrelationIdReturned()
        {
            //Assert.That(_context., Is.EqualTo(_correlationId));
        }
    }
}