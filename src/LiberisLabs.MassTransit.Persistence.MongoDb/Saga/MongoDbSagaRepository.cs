using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Logging;
using MassTransit.Pipeline;
using MassTransit.Saga;
using MassTransit.Util;
using MongoDB.Driver;

namespace MassTransit.Persistence.MongoDb.Saga
{
    public class MongoDbSagaRepository<TSaga> : ISagaRepository<TSaga>
        where TSaga : class, IVersionedSaga
    {
        private static readonly ILog _log = Logger.Get<MongoDbSagaRepository<TSaga>>();
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
                await PreInsertSagaInstance(context, instance).ConfigureAwait(false);
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
                await SendToInstance(context, policy, next, instance).ConfigureAwait(false);
            }
        }

        private async Task PreInsertSagaInstance<T>(ConsumeContext<T> context, TSaga instance) where T : class
        {
            try
            {
                await _collection.InsertOneAsync(instance, cancellationToken: context.CancellationToken).ConfigureAwait(false);

                _log.DebugFormat("SAGA:{0}:{1} Insert {2}", TypeMetadataCache<TSaga>.ShortName, instance.CorrelationId, TypeMetadataCache<T>.ShortName);
            }
            catch (Exception ex)
            {
                if (_log.IsDebugEnabled)
                {
                    _log.DebugFormat("SAGA:{0}:{1} Dupe {2} - {3}", TypeMetadataCache<TSaga>.ShortName, instance.CorrelationId,
                        TypeMetadataCache<T>.ShortName, ex.Message);
                }
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
                        await SendToInstance(context, policy, next, instance).ConfigureAwait(false);
                    }
                }
            }
            catch (SagaException sex)
            {
                if (_log.IsErrorEnabled)
                    _log.Error("Saga Exception Occurred", sex);
            }
            catch (Exception ex)
            {
                if (_log.IsErrorEnabled)
                    _log.Error($"SAGA:{TypeMetadataCache<TSaga>.ShortName} Exception {TypeMetadataCache<T>.ShortName}", ex);

                throw new SagaException(ex.Message, typeof(TSaga), typeof(T), Guid.Empty, ex);
            }
        }

        private async Task SendToInstance<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next, TSaga instance) where T : class
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.DebugFormat("SAGA:{0}:{1} Used {2}", TypeMetadataCache<TSaga>.ShortName, instance.CorrelationId, TypeMetadataCache<T>.ShortName);
                }

                var sagaConsumeContext = _mongoDbSagaConsumeContextFactory.Create(_collection, context, instance);

                await policy.Existing(sagaConsumeContext, next).ConfigureAwait(false);

                await UpdateMongoDbSaga(context, instance).ConfigureAwait(false);
            }
            catch (SagaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SagaException(ex.Message, typeof(TSaga), typeof(T), instance.CorrelationId, ex);
            }
        }

        private async Task UpdateMongoDbSaga(PipeContext context, TSaga instance)
        {
            instance.Version++;

            var old = await _collection.FindOneAndReplaceAsync(x => x.CorrelationId == instance.CorrelationId && x.Version < instance.Version, instance, cancellationToken: context.CancellationToken).ConfigureAwait(false);

            if (old == null)
            {
                throw new MongoDbConcurrencyException("Unable to update saga. It may not have been found or may have been updated by another process.");
            }
        }
    }
}