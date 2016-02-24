using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit.Saga;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbQuerySagaRepository<TSaga> : IQuerySagaRepository<TSaga> where TSaga : class, ISaga
    {
        public Task<IEnumerable<Guid>> Find(ISagaQuery<TSaga> query)
        {
            return Task.FromResult(new[] {Guid.NewGuid()} as IEnumerable<Guid>);
            throw new NotImplementedException();
        }
    }
}