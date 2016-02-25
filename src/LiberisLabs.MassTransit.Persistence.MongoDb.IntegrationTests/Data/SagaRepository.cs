using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LiberisLabs.MassTransit.Persistence.MongoDb.IntegrationTests.Data
{
    public static class SagaRepository
    {
         public static IMongoDatabase Instance => new MongoClient("mongodb://127.0.0.1").GetDatabase("sagaTest");

        public static async Task DeleteSaga(Guid correlationId)
        {
            await Instance.GetCollection<BsonDocument>("sagas").DeleteOneAsync(x => x["correlationId"] == correlationId);
        }
    }
}