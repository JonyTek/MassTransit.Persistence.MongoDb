using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Pipeline;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MissingPipeTests
{
    [TestFixture]
    public class MissingPipeTestsForProbing
    {
        private Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>> _nextPipe;
        private MissingPipe<SimpleSaga, InitiateSimpleSaga> _pipe;
        private Mock<ProbeContext> _probeContext;

        [OneTimeSetUp]
        public void GivenAMissingPipe()
        {
            _probeContext = new Mock<ProbeContext>();

            _nextPipe = new Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>();

            _pipe = new MissingPipe<SimpleSaga, InitiateSimpleSaga>(Mock.Of<IMongoCollection<SimpleSaga>>(), _nextPipe.Object, Mock.Of<IMongoDbSagaConsumeContextFactory>());
        }

        [SetUp]
        public void WhenProbing()
        {
            _pipe.Probe(_probeContext.Object);
        }

        [Test]
        public void ThenNextPipeProbed()
        {
            _nextPipe.Verify(m => m.Probe(_probeContext.Object), Times.Once);
        }
    }
}