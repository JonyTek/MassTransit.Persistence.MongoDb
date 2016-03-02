using LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MongoDbSagaRepositoryTests
{
    [TestFixture]
    public class MongoDbSagaRepositoryTestsForProbing
    {
        private Mock<ProbeContext> _probeContext;
        private Mock<ProbeContext> _scope;

        [OneTimeSetUp]
        public void GivenAMongoDbSagaRepository_WhenProbing()
        {
            _scope = new Mock<ProbeContext>();

            _probeContext = new Mock<ProbeContext>();
            _probeContext.Setup(m => m.CreateScope("sagaRepository")).Returns(_scope.Object);

            var repository = new MongoDbSagaRepository<SimpleSaga>(SagaRepository.Instance, null);

            repository.Probe(_probeContext.Object);
        }

        [Test]
        public void ThenScopeIsReturned()
        {
            _probeContext.Verify(m => m.CreateScope("sagaRepository"));
        }

        [Test]
        public void ThenScopeIsSet()
        {
            _scope.Verify(x => x.Set(It.IsAny<object>()));
        }
    }
}