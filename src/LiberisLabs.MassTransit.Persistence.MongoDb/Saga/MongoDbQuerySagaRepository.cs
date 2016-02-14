using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit.Saga;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbQuerySagaRepository<TSaga> : IQuerySagaRepository<TSaga> where TSaga : class, ISaga
    {
        Task<IEnumerable<Guid>> IQuerySagaRepository<TSaga>.Find(ISagaQuery<TSaga> query)
        {
            throw new NotImplementedException();
        }
    }
}