using System;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Data;
using MassTransit.Persistence.MongoDb.Saga;
using MongoDB.Driver;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Tests
{
    [TestFixture]
    public class LocatingAnExistingSagaTests
    {
        private Guid _correlationId;
        private InitiateSimpleSaga _message;
        private Guid? _foundId;

        [OneTimeSetUp]
        public async Task GivenASimpleSaga_WhenQueryingSagaRepositoryForSaga()
        {
            _correlationId = Guid.NewGuid();
            _message = new InitiateSimpleSaga(_correlationId);

            var busControl = await Bus.StartAsync();

            await busControl.Publish(_message);

            var sagaRepository = new MongoDbQuerySagaRepository<SimpleSaga>(SagaRepository.Instance);

            _foundId = await sagaRepository.ShouldContainSaga(_correlationId, TimeSpan.FromSeconds(30));
        }

        [Test]
        public void ThenSagaIsFound()
        {
            Assert.That(_foundId, Is.EqualTo(_message.CorrelationId));
        }

        [Test]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_correlationId);
        }
    }
}
