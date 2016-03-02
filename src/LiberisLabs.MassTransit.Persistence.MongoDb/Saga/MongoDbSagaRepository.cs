using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Pipeline;
using MassTransit.Saga;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbSagaRepository<TSaga> : ISagaRepository<TSaga>
        where TSaga : class, IVersionedSaga
    {
        private readonly IMongoDbSagaConsumeContextFactory _mongoDbSagaConsumeContextFactory;
        private readonly IMongoCollection<TSaga> _collection;

        public MongoDbSagaRepository(string connectionString, string database)
            : this(new MongoClient(connectionString).GetDatabase(database), new MongoDbSagaConsumeContextFactory())
        {
        }

        public MongoDbSagaRepository(MongoUrl mongoUrl)
            : this(mongoUrl.Url, mongoUrl.DatabaseName)
        {
        }

        public MongoDbSagaRepository(IMongoDatabase mongoDatabase, IMongoDbSagaConsumeContextFactory mongoDbSagaConsumeContextFactory)
        {
            _mongoDbSagaConsumeContextFactory = mongoDbSagaConsumeContextFactory;
            _collection = mongoDatabase.GetCollection<TSaga>("sagas");
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("sagaRepository");

            scope.Set(new
            {
                Persistence = "mongodb",
                //Mongo is schemaless so assume that the class contains all entities 
                Entities = typeof(TSaga).GetProperties().Select(x => x.Name)
            });
        }

        public async Task Send<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            if (!context.CorrelationId.HasValue)
                throw new SagaException("The CorrelationId was not specified", typeof(TSaga), typeof(T));

            TSaga instance;

            if (policy.PreInsertInstance(context, out instance))
            {
                await TryInsertInstance(context, instance).ConfigureAwait(false);
            }

            if (instance == null)
            {
                instance = await _collection.Find(x => x.CorrelationId == context.CorrelationId).SingleOrDefaultAsync(context.CancellationToken).ConfigureAwait(false);
            }

            if (instance == null)
            {
                var missingSagaPipe = new MissingPipe<TSaga, T>(_collection, next, _mongoDbSagaConsumeContextFactory);

                await policy.Missing(context, missingSagaPipe).ConfigureAwait(false);
            }
            else
            {
                var sagaConsumeContext = _mongoDbSagaConsumeContextFactory.Create(_collection, context, instance);

                await policy.Existing(sagaConsumeContext, next).ConfigureAwait(false);

                await _collection.FindOneAndReplaceAsync(x => x.CorrelationId == context.CorrelationId && x.Version < instance.Version, instance, cancellationToken: context.CancellationToken).ConfigureAwait(false);
            }
        }

        private async Task TryInsertInstance<T>(ConsumeContext<T> context, TSaga instance) where T : class
        {
            try
            {
                await _collection.InsertOneAsync(instance, cancellationToken: context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //Log
            }
        }

        public async Task SendQuery<T>(SagaQueryConsumeContext<TSaga, T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            try
            {
                var sagaInstances = await _collection.Find(context.Query.FilterExpression).ToListAsync().ConfigureAwait(false);

                if (!sagaInstances.Any())
                {
                    var missingPipe = new MissingPipe<TSaga, T>(_collection, next, _mongoDbSagaConsumeContextFactory);

                    await policy.Missing(context, missingPipe).ConfigureAwait(false);
                }
                else
                {
                    foreach (var instance in sagaInstances)
                    {
                        try
                        {
                            var sagaConsumeContext = _mongoDbSagaConsumeContextFactory.Create(_collection, context, instance);

                            await policy.Existing(sagaConsumeContext, next).ConfigureAwait(false);
                        }
                        catch (SagaException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new SagaException(ex.Message, typeof(TSaga), typeof(T), instance.CorrelationId, ex);
                        }

                        await _collection.FindOneAndReplaceAsync(x => x.CorrelationId == instance.CorrelationId && x.Version < instance.Version, instance, cancellationToken: context.CancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (SagaException)
            {
                //Log
            }
            catch (Exception ex)
            {
                throw new SagaException(ex.Message, typeof(TSaga), typeof(T), Guid.Empty, ex);
            }
        }
    }
}