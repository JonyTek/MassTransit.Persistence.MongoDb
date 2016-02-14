using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit.Pipeline;
using MassTransit.Saga;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbSagaRepository<TSaga> : ISagaRepository<TSaga> where TSaga : class, ISaga
    {
        public void Probe(ProbeContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task Send<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task SendQuery<T>(SagaQueryConsumeContext<TSaga, T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task Find(ISagaQuery<TSaga> query)
        {
            throw new System.NotImplementedException();
        }
    }
}