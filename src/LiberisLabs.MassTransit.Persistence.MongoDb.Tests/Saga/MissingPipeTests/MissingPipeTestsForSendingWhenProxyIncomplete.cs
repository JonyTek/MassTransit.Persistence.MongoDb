using System;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Pipeline;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MissingPipeTests
{
    [TestFixture]
    public class MissingPipeTestsForSendingWhenProxyIncomplete
    {
        private Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>> _nextPipe;
        private MissingPipe<SimpleSaga, InitiateSimpleSaga> _pipe;
        private Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>> _context;
        private Mock<IMongoDbSagaConsumeContextFactory> _consumeContextFactory;
        private Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>> _proxy;
        private SimpleSaga _saga;

        [OneTimeSetUp]
        public async Task GivenAMissingPipe_WhenSendingAndProxyIncomplete()
        {
            var collection = SagaRepository.Instance.GetCollection<SimpleSaga>("sagas");
            _nextPipe = new Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>();
            _proxy = new Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>();
            _proxy.SetupGet(m => m.IsCompleted).Returns(false);
            _consumeContextFactory = new Mock<IMongoDbSagaConsumeContextFactory>();
            _saga = new SimpleSaga {CorrelationId = Guid.NewGuid()};
            _context = new Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>();
            _context.SetupGet(m => m.Saga).Returns(_saga);
            _consumeContextFactory.Setup(m => m.Create(collection, _context.Object, _context.Object.Saga, false)).Returns(_proxy.Object);

            _pipe = new MissingPipe<SimpleSaga, InitiateSimpleSaga>(collection, _nextPipe.Object, _consumeContextFactory.Object);

            await _pipe.Send(_context.Object);
        }

        [Test]
        public void ThenNextPipeCalled()
        {
            _nextPipe.Verify(m => m.Send(It.IsAny<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>()), Times.Once);
        }

        [Test]
        public async Task ThenSagaInsertedIntoMongo()
        {
            var saga = await SagaRepository.GetSaga(_saga.CorrelationId);

            Assert.That(saga, Is.Not.Null);
        }
    }
}