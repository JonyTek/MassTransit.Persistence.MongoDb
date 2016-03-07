using System;
using System.Threading;
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
    public class MongoDbSagaRepositoryTestsForSendingWhenInstanceNotFound
    {
        private Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>> _policy;
        private Mock<ConsumeContext<InitiateSimpleSaga>> _context;
        private SimpleSaga _nullSimpleSaga;
        private Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>> _nextPipe;

        [OneTimeSetUp]
        public async Task GivenAMongoDbSagaRepository_WhenSendingAndInstanceNotFound()
        {
            _context = new Mock<ConsumeContext<InitiateSimpleSaga>>();
            _context.Setup(x => x.CorrelationId).Returns(It.IsAny<Guid>());
            _context.Setup(m => m.CancellationToken).Returns(It.IsAny<CancellationToken>());

            _nullSimpleSaga = null;

            _policy = new Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>>();
            _policy.Setup(x => x.PreInsertInstance(_context.Object, out _nullSimpleSaga)).Returns(false);

            _nextPipe = new Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>();

            var repository = new MongoDbSagaRepository<SimpleSaga>(SagaRepository.Instance, null);

            await repository.Send(_context.Object, _policy.Object, _nextPipe.Object);
        }

        [Test]
        public void ThenMissingPipeInvokedOnPolicy()
        {
            _policy.Verify(m => m.Missing(_context.Object, It.IsAny<MissingPipe<SimpleSaga, InitiateSimpleSaga>>()));
        }
    }
}
