using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Saga;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga
{
    [TestFixture]
    public class MongoDbQuerySagaRepositoryTests
    {
        private Guid _correlationId;
        private IEnumerable<Guid> _result;

        [OneTimeSetUp]
        public async Task GivenAMongoDbQuerySagaRepository_WhenFindingSaga()
        {
            _correlationId = Guid.NewGuid();

            await SagaRepository.InsertSaga(new SimpleSaga() {CorrelationId = _correlationId});

            var repository = new MongoDbQuerySagaRepository<SimpleSaga>(SagaRepository.Instance);

            var sagaQuery = new Mock<ISagaQuery<SimpleSaga>>();
            sagaQuery.Setup(m => m.FilterExpression).Returns(x => x.CorrelationId == _correlationId);

            _result = await repository.Find(sagaQuery.Object);
        }

        [Test]
        public void ThenCorrelationIdsReturned()
        {
            Assert.That(_result.Single(), Is.EqualTo(_correlationId));
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_correlationId);
        }
    }
}