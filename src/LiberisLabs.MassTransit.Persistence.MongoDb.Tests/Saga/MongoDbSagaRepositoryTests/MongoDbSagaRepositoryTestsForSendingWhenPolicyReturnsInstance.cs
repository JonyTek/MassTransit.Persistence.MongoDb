using System;
using System.Threading;
using System.Threading.Tasks;
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
    public class MongoDbSagaRepositoryTestsForSendingWhenPolicyReturnsInstance
    {
        private Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>> _policy;
        private Mock<ConsumeContext<InitiateSimpleSaga>> _context;
        private SimpleSaga _simpleSaga;
        private Guid _correlationId;
        private Mock<IMongoCollection<SimpleSaga>> _collection;
        private CancellationToken _cancellationToken;
        private Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>> _nextPipe;

        [OneTimeSetUp]
        public async Task GivenAMongoDbSagaRepository_WhenSendingAndPolicyReturnsInstance()
        {
            _correlationId = Guid.NewGuid();
            _cancellationToken = new CancellationToken();

            _context = new Mock<ConsumeContext<InitiateSimpleSaga>>();
            _context.Setup(x => x.CorrelationId).Returns(_correlationId);
            _context.Setup(m => m.CancellationToken).Returns(_cancellationToken);

            _simpleSaga = new SimpleSaga();

            _policy = new Mock<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>>();
            _policy.Setup(x => x.PreInsertInstance(_context.Object, out _simpleSaga)).Returns(true);

            _nextPipe = new Mock<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>();

            var database = new Mock<IMongoDatabase>();
            _collection = new Mock<IMongoCollection<SimpleSaga>>();
            database.Setup(m => m.GetCollection<SimpleSaga>("sagas", null)).Returns(_collection.Object);

            var repository = new MongoDbSagaRepository<SimpleSaga>(database.Object);

            await repository.Send(_context.Object, _policy.Object, _nextPipe.Object);
        }

        [Test]
        public void ThenPreInsertInstanceCalledToGetInstance()
        {
            _policy.Verify(m => m.PreInsertInstance(_context.Object, out _simpleSaga));
        }

        [Test]
        public void ThenSagaInstanceStored()
        {
            _collection.Verify(m => m.InsertOneAsync(_simpleSaga, It.IsAny<InsertOneOptions>(), _cancellationToken));
        }

        [Test]
        public void ThenPolicyUpdatedWithSagaInstance()
        {
            _policy.Verify(m => m.Existing(It.IsAny<MongoDbSagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>(), _nextPipe.Object));
        }
    }
}
