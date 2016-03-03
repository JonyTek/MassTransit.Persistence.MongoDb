using System;
using System.Threading;
using System.Threading.Tasks;
using LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Data;
using MassTransit.Persistence.MongoDb.Saga;
using MongoDB.Driver;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Tests
{
    [TestFixture]
    public class InitiatingAndObservingSagaTests
    {
        private Guid _correlationId;

        [Test]
        public async Task GivenACorrelatedMessage_WhenInitiatingAndObservedMessageForSagaArrives_ThenSagaShouldBeLoaded()
        {
            _correlationId = Guid.NewGuid();
            var initiationMessage = new InitiateSimpleSaga(_correlationId) {Name = "Lee"};

            var busControl = await Bus.StartAsync();

            await busControl.Publish(initiationMessage);

            var sagaRepository = new MongoDbQuerySagaRepository<SimpleSaga>(SagaRepository.Instance);

            var foundId = await sagaRepository.ShouldContainSaga(x => x.Initiated && x.CorrelationId == _correlationId, TimeSpan.FromSeconds(30));

            var observableMessage = new ObservableSagaMessage {Name = "Lee"};

            await busControl.Publish(observableMessage);

            foundId = await sagaRepository.ShouldContainSaga(x => x.Observed && x.CorrelationId == _correlationId, TimeSpan.FromSeconds(300));

            Assert.That(foundId.HasValue, Is.True);
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            await SagaRepository.DeleteSaga(_correlationId);
        }
    }
}
