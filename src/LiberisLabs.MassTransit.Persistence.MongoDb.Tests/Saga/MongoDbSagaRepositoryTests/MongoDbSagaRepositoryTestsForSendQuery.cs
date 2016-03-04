using System;
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
    public class MongoDbSagaRepositoryTestsForSendQuery
    {
        private Guid _correlationId;
        private Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>> _sagaPolicy;
        private Mock<SagaQueryConsumeContext<SimpleSaga, InitiateSimpleSaga>> _sagaQueryConsumeContext;
        private Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>> _nextPipe;
        private Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>> _sagaConsumeContext;
        private Mock<IMongoDbSagaConsumeContextFactory> _sagaConsumeContextFactory;

        [OneTimeSetUp]
        public async Task GivenAMongoDbSagaRepository_WhenSendingQuery()
        {
            _correlationId = Guid.NewGuid();
            var saga = new SimpleSaga {CorrelationId = _correlationId};

            await SagaRepository.InsertSaga(saga);

            _sagaQueryConsumeContext = new Mock<SagaQueryConsumeContext<SimpleSaga, InitiateSimpleSaga>>();
            _sagaQueryConsumeContext.Setup(x => x.Query.FilterExpression).Returns(x => x.CorrelationId == _correlationId);
            _sagaPolicy = new Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>>();
            _nextPipe = new Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>();

            _sagaConsumeContext = new Mock<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>();
            _sagaConsumeContext.Setup(x => x.CorrelationId).Returns(_correlationId);

            _sagaConsumeContextFactory = new Mock<IMongoDbSagaConsumeContextFactory>();
            _sagaConsumeContextFactory.Setup(m => m.Create(It.IsAny<IMongoCollection<SimpleSaga>>(), _sagaQueryConsumeContext.Object, It.Is<SimpleSaga>(x => x.CorrelationId == _correlationId), true)).Returns(_sagaConsumeContext.Object);

            var repository = new MongoDbSagaRepository<SimpleSaga>(SagaRepository.Instance, _sagaConsumeContextFactory.Object);

            await repository.SendQuery(_sagaQueryConsumeContext.Object, _sagaPolicy.Object, _nextPipe.Object);
        }

        [Test]
        public void ThenSagaSentToInstance()
        {
            _sagaPolicy.Verify(x => x.Existing(_sagaConsumeContext.Object, _nextPipe.Object));
        }

        [Test]
        public void ThenMissingPipeNotCalled()
        {
            _sagaPolicy.Verify(x => x.Missing(_sagaQueryConsumeContext.Object, It.IsAny<MissingPipe<SimpleSaga, InitiateSimpleSaga>>()), Times.Never);
        }

        [Test]
        public async Task ThenVersionIncremeted()
        {
            var saga = await SagaRepository.GetSaga(_correlationId);

            Assert.That(saga.Version, Is.EqualTo(1));
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_correlationId);
        }
    }
}