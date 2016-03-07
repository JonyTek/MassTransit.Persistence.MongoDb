using System;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Data;
using MassTransit.Persistence.MongoDb.Saga;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Tests
{
    [TestFixture]
    public class StartingSagaTests
    {
        private Guid? _foundId;
        private Guid _correlationId;

        [OneTimeSetUp]
        public async Task GivenAnInitiatingMessage_WhenPublishing()
        {
            _correlationId = Guid.NewGuid();
            var message = new InitiateSimpleSaga(_correlationId);

            var busControl = await Bus.StartAsync();

            await busControl.Publish(message);

            var sagaRepository = new MongoDbQuerySagaRepository<SimpleSaga>(SagaRepository.Instance);

            _foundId = await sagaRepository.ShouldContainSaga(_correlationId, TimeSpan.FromSeconds(30));
        }

        [Test]
        public void ThenTheSagaShouldBeStarted()
        {
            Assert.That(_foundId.Value, Is.EqualTo(_correlationId));
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_correlationId);
        }
    }
}