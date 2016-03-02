using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Persistence.MongoDb.Saga;
using MassTransit.Saga;
using MongoDB.Bson.Serialization.Attributes;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    public class SimpleSaga :
        InitiatedBy<InitiateSimpleSaga>,
        Orchestrates<CompleteSimpleSaga>,
        Observes<ObservableSagaMessage, SimpleSaga>,
        IVersionedSaga
    {
        public bool Completed { get; private set; }

        public bool Initiated { get; private set; }

        public bool Observed { get; private set; }

        public string Name { get; private set; }
        
        public Task Consume(ConsumeContext<InitiateSimpleSaga> context)
        {
            Version++;

            Initiated = true;
            Name = context.Message.Name;

            return Task.FromResult(0);
        }

        [BsonId]
        public Guid CorrelationId { get; set; }

        public Task Consume(ConsumeContext<ObservableSagaMessage> message)
        {
            Version++;

            Observed = true;

            return Task.FromResult(0);
        }

        public Expression<Func<SimpleSaga, ObservableSagaMessage, bool>> CorrelationExpression
        {
            get { return (saga, message) => saga.Name == message.Name; }
        }

        public Task Consume(ConsumeContext<CompleteSimpleSaga> message)
        {
            Version++;

            Completed = true;

            return Task.FromResult(0);
        }

        public int Version { get; private set; }
    }
}