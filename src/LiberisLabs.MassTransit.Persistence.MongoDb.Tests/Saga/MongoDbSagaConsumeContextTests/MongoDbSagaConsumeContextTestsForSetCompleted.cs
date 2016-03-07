using System;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MongoDbSagaConsumeContextTests
{
    [TestFixture]
    public class MongoDbSagaConsumeContextTestsForSetCompleted
    {
        private SimpleSaga _saga;
        private MongoDbSagaConsumeContext<SimpleSaga, InitiateSimpleSaga> _mongoDbSagaConsumeContext;

        [OneTimeSetUp]
        public async Task GivenAMongoDbSagaConsumeContext_WhenSettingComplete()
        {
            _saga = new SimpleSaga {CorrelationId = Guid.NewGuid()};

            await SagaRepository.InsertSaga(_saga);

            _mongoDbSagaConsumeContext = new MongoDbSagaConsumeContext<SimpleSaga, InitiateSimpleSaga>(SagaRepository.Instance.GetCollection<SimpleSaga>("sagas"), Mock.Of<ConsumeContext<InitiateSimpleSaga>>(), _saga);

            await _mongoDbSagaConsumeContext.SetCompleted();
        }
        
        [Test]
        public void ThenContextIsSetToComplete()
        {
            Assert.That(_mongoDbSagaConsumeContext.IsCompleted, Is.True);
        }

        [Test]
        public async Task ThenSagaDoesNotExistInRepository()
        {
            var saga = await SagaRepository.GetSaga(_saga.CorrelationId);

            Assert.That(saga, Is.Null);
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_saga.CorrelationId);
        }
    }
}