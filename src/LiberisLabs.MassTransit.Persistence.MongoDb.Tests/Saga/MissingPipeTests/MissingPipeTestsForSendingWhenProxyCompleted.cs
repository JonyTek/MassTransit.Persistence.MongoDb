using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Pipeline;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MissingPipeTests
{
    [TestFixture]
    public class MissingPipeTestsForSendingWhenProxyCompleted
    {
        private Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>> _nextPipe;
        private MissingPipe<SimpleSaga, InitiateSimpleSaga> _pipe;
        private Mock<IMongoCollection<SimpleSaga>> _collection;
        private Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>> _context;
        private Mock<IMongoDbSagaConsumeContextFactory> _consumeContextFactory;
        private Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>> _proxy;

        [OneTimeSetUp]
        public async Task GivenAMissingPipe_WhenSendingAndProxyCompleted()
        {
            _collection = new Mock<IMongoCollection<SimpleSaga>>();
            _nextPipe = new Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>();
            _proxy = new Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>();
            _proxy.SetupGet(m => m.IsCompleted).Returns(true);
            _consumeContextFactory = new Mock<IMongoDbSagaConsumeContextFactory>();
            _context = new Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>();
            _consumeContextFactory.Setup(m => m.Create(_collection.Object, _context.Object, _context.Object.Saga, false)).Returns(_proxy.Object);

            _pipe = new MissingPipe<SimpleSaga, InitiateSimpleSaga>(_collection.Object, _nextPipe.Object, _consumeContextFactory.Object);

            await _pipe.Send(_context.Object);
        }

        [Test]
        public void ThenNextPipeCalled()
        {
            _nextPipe.Verify(m => m.Send(It.IsAny<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>()), Times.Once);
        }

        [Test]
        public void ThenSagaNotInsertedIntoCollection()
        {
            _collection.Verify(m => m.InsertOneAsync(It.IsAny<SimpleSaga>(), null, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}