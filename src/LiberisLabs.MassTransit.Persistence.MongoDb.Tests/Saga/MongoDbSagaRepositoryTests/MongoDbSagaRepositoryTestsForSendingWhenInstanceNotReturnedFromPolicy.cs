using System;
using System.Threading;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Pipeline;
using MassTransit.Saga;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MongoDbSagaRepositoryTests
{
    [TestFixture]
    public class MongoDbSagaRepositoryTestsForSendingWhenInstanceNotReturnedFromPolicy
    {
        private Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>> _policy;
        private Mock<ConsumeContext<InitiateSimpleSaga>> _context;
        private SimpleSaga _nullSimpleSaga;
        private Guid _correlationId;
        private CancellationToken _cancellationToken;
        private Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>> _nextPipe;
        private SimpleSaga _simpleSaga;
        private Mock<IMongoDbSagaConsumeContextFactory> _sagaConsumeContextFactory;
        private Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>> _sagaConsumeContext;

        [OneTimeSetUp]
        public async Task GivenAMongoDbSagaRepository_WhenSendingAndInstanceNotReturnedFromPolicy()
        {
            _correlationId = Guid.NewGuid();
            _cancellationToken = new CancellationToken();

            _context = new Mock<ConsumeContext<InitiateSimpleSaga>>();
            _context.Setup(x => x.CorrelationId).Returns(_correlationId);
            _context.Setup(m => m.CancellationToken).Returns(_cancellationToken);

            _nullSimpleSaga = null;

            _policy = new Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>>();
            _policy.Setup(x => x.PreInsertInstance(_context.Object, out _nullSimpleSaga)).Returns(false);

            _nextPipe = new Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>();

            _simpleSaga = new SimpleSaga {CorrelationId = _correlationId};

            _sagaConsumeContext = new Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>();
            _sagaConsumeContext.Setup(x => x.CorrelationId).Returns(_correlationId);

            _sagaConsumeContextFactory = new Mock<IMongoDbSagaConsumeContextFactory>();
            _sagaConsumeContextFactory.Setup(m => m.Create(It.IsAny<IMongoCollection<SimpleSaga>>(), _context.Object, It.Is<SimpleSaga>(x => x.CorrelationId == _correlationId), true)).Returns(_sagaConsumeContext.Object);

            await SagaRepository.InsertSaga(_simpleSaga);

            var repository = new MongoDbSagaRepository<SimpleSaga>(SagaRepository.Instance, _sagaConsumeContextFactory.Object);

            await repository.Send(_context.Object, _policy.Object, _nextPipe.Object);
        }

        [Test]
        public void ThenPreInsertInstanceCalledToGetInstance()
        {
            _policy.Verify(m => m.PreInsertInstance(_context.Object, out _nullSimpleSaga));
        }

        [Test]
        public void ThenPolicyUpdatedWithSagaInstance()
        {
            _policy.Verify(m => m.Existing(_sagaConsumeContext.Object, _nextPipe.Object));
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_correlationId);
        }
    }
}
