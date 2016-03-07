using System;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests
{
    public class CompleteSimpleSaga :
        SimpleSagaMessageBase
    {
        public CompleteSimpleSaga()
        {
        }

        public CompleteSimpleSaga(Guid correlationId)
            : base(correlationId)
        {
        }
    }
}