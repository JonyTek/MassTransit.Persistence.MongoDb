using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Saga;
using MassTransit.Transports.InMemory;
using MongoDB.Driver;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Tests
{
    [TestFixture]
    public class LocatingAnExistingSagaTests
    {
        private Guid _sagaId;
        private InitiateSimpleSaga _message;
        private Guid _foundId;

        [OneTimeSetUp]
        public async Task GivenASimpleSaga_WhenQueryingSagaRepositoryForSaga()
        {
            _sagaId = Guid.NewGuid();
            _message = new InitiateSimpleSaga(_sagaId);

            var busControl = Bus.Factory.CreateUsingInMemory(configurator =>
            {
                configurator.ReceiveEndpoint("input_queue",
                    endpointConfigurator =>
                    {
                        endpointConfigurator.Saga(new MongoDbSagaRepository<SimpleSaga>(new MongoUrl("mongodb://localhost/sagaTest")));
                    });
            });

            await busControl.StartAsync();

            await busControl.Publish(_message);

            Thread.Sleep(1000);

            var sagaRepository = new MongoDbQuerySagaRepository<SimpleSaga>();
            _foundId = (await sagaRepository.Find(new SagaQuery<SimpleSaga>(x => x.CorrelationId == _message.CorrelationId))).Single();
        }

        [Test]
        public void ThenSagaIsFound()
        {
            Assert.That(_foundId, Is.EqualTo(_message.CorrelationId));
        }
    }
}
