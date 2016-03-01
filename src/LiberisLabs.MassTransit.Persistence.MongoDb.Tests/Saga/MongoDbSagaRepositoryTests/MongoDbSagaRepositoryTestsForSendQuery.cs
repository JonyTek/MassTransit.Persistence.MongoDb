using System;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Pipeline;
using MassTransit.Saga;
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

            var repository = new MongoDbSagaRepository<SimpleSaga>(SagaRepository.Instance);

            await repository.SendQuery(_sagaQueryConsumeContext.Object, _sagaPolicy.Object, _nextPipe.Object);
        }

        [Test]
        public void ThenSagaSentToInstance()
        {
            _sagaPolicy.Verify(x => x.Existing(It.IsAny<MongoDbSagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>(), _nextPipe.Object));
        }

        [Test]
        public void ThenMissingPipeNotCalled()
        {
            _sagaPolicy.Verify(x => x.Missing(_sagaQueryConsumeContext.Object, It.IsAny<MissingPipe<SimpleSaga, InitiateSimpleSaga>>()), Times.Never);
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_correlationId);
        }
    }
}