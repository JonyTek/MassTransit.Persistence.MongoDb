using System;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests
{
    public class CompleteSimpleSaga :
        SimpleSagaMessageBase
    {
        public CompleteSimpleSaga(Guid correlationId)
            : base(correlationId)
        {
        }
    }
}