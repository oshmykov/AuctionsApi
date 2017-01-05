using AuctionsApi.Models.Data.Abstract.Mongo;
using MongoDB.Driver;

namespace AuctionsApi.Models.Data.Impl.Mongo
{
    public sealed class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoClient client;
        private readonly IMongoDatabase database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            client = new MongoClient(connectionString);
            database = client.GetDatabase(databaseName);
        }

        public IMongoClient Client
        {
            get
            {
                return client;
            }
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return database.GetCollection<T>(collectionName);
        }
    }
}
