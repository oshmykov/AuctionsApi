using MongoDB.Driver;

namespace AuctionsApi.Models.Data.Abstract.Mongo
{
    public interface IMongoDbContext
    {
        IMongoClient Client { get; }
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
