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
        [Test]
        public async Task GivenACorrelatedMessage_TheCorrectSagaShouldBeFound()
        {
            var correlationId = Guid.NewGuid();
            var message = new InitiateSimpleSaga(correlationId);

            var busControl = await Bus.StartAsync();

            await busControl.Publish(message);

            var sagaRepository = new MongoDbQuerySagaRepository<SimpleSaga>(SagaRepository.Instance);

            var foundId = await sagaRepository.ShouldContainSaga(correlationId, TimeSpan.FromSeconds(5));

            Assert.That(foundId.HasValue, Is.True);

            var nextMessage = new CompleteSimpleSaga(correlationId);

            await busControl.Publish(nextMessage);

            foundId = await sagaRepository.ShouldContainSaga(x => x.CorrelationId == correlationId && x.Completed, TimeSpan.FromSeconds(5));

            Assert.That(foundId.HasValue, Is.True);
        }
    }
}
