using System;
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
    public class MongoDbSagaRepositoryTestsForSendingWhenCorrelationIdNull
    {
        private SagaException _exception;

        [OneTimeSetUp]
        public async Task GivenAMongoDbSagaRepository_WhenSendingWithNullCorrelationId()
        {
            var context = new Mock<ConsumeContext<InitiateSimpleSaga>>();
            context.Setup(x => x.CorrelationId).Returns(default(Guid?));

            var repository = new MongoDbSagaRepository<SimpleSaga>(Mock.Of<IMongoDatabase>());

            try
            {
                await repository.Send(context.Object, Mock.Of<ISagaPolicy<SimpleSaga, InitiateSimpleSaga>>(), Mock.Of<IPipe<SagaConsumeContext<SimpleSaga, InitiateSimpleSaga>>>());
            }
            catch (SagaException exception)
            {
                _exception = exception;
            }
        }

        [Test]
        public void ThenSagaExceptionThrown()
        {
            Assert.That(_exception, Is.Not.Null);
        }
    }
}
