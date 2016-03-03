using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.Tests.Data
{
    public static class SagaRepository
    {
        public static IMongoDatabase Instance => new MongoClient("mongodb://127.0.0.1").GetDatabase("sagaTest");

        public static async Task InsertSaga(SimpleSaga saga)
        {
            await Instance.GetCollection<SimpleSaga>("sagas").InsertOneAsync(saga);
        }

        public static async Task DeleteSaga(Guid correlationId)
        {
            await Instance.GetCollection<BsonDocument>("sagas").DeleteOneAsync(x => x["_id"] == correlationId);
        }

        public static async Task<SimpleSaga> GetSaga(Guid correlationId)
        {
            return await Instance.GetCollection<SimpleSaga>("sagas").Find(x => x.CorrelationId == correlationId).SingleOrDefaultAsync();
        }
    }
}