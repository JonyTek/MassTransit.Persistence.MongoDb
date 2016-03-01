using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Saga.MongoDbSagaConsumeContextTests
{
    [TestFixture]
    public class MongoDbSagaConsumeContextTestsForPopContext
    {
        private SagaConsumeContext<SimpleSaga, InitiateSimpleSaga> _context;

        [OneTimeSetUp]
        public void GivenAMongoDbSagaConsumeContext_WhenPoppingContext()
        {
            var mongoDbSagaConsumeContext = new MongoDbSagaConsumeContext<SimpleSaga, InitiateSimpleSaga>(Mock.Of<IMongoCollection<SimpleSaga>>(), Mock.Of<ConsumeContext<InitiateSimpleSaga>>(), Mock.Of<SimpleSaga>());

            _context = mongoDbSagaConsumeContext.PopContext<InitiateSimpleSaga>();
        }

        [Test]
        public void ThenMongoContextReturnedAsSagaConsumeContext()
        {
            Assert.That(_context, Is.Not.Null);
        }
    }
}