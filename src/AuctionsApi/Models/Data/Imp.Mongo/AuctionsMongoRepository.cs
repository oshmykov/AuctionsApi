using AuctionsApi.Models.Data.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System.Linq.Expressions;
using MongoDB.Driver;
using AuctionsApi.Models.Data.Abstract.Mongo;
using System.Threading;

namespace AuctionsApi.Models.Data.Impl.Mongo
{
    public class AuctionsMongoRepository : IRepository<AuctionDoc>
    {
        private const string COLLECTION_NAME = "auctions";

        private IList<AuctionDoc> updates;

        protected const string EXPIRATION = "expiration";
        private const string NAME = "name";

        protected const string INVALID_AUCTION_ID = "Invalid auction Id";

        private IMongoDbContext dbContext;

        public bool HasPendingChanges
        {
            get
            {
                return updates.Count > 0;
            }
        }

        public AuctionsMongoRepository(IMongoDbContext dbContext)
        {
            this.dbContext = dbContext;
            updates = new List<AuctionDoc>();
        }

        public virtual async Task<IEnumerable<AuctionDoc>> ReadAsync(
            Expression<Func<AuctionDoc, bool>> filter = null,
            string sortBy = EXPIRATION,
            bool ascending = true,
            int? skipAmount = default(int?),
            int? limitAmount = default(int?))
        {
            var query = GetCollection().AsQueryable().Where(filter);

            if (NAME.Equals(sortBy))
            {
                query = ascending
                    ? query.OrderBy(a => a.Name)
                    : query.OrderByDescending(a => a.Name);
            }
            else
            {
                query = ascending
                    ? query.OrderBy(a => a.ExpiresAtUtc)
                    : query.OrderByDescending(a => a.ExpiresAtUtc);
            }

            if (skipAmount.HasValue && skipAmount.Value > 0)
            {
                query = query.Skip(skipAmount.Value);
            }

            if (limitAmount.HasValue && limitAmount.Value > 0)
            {
                query = query.Take(limitAmount.Value);
            }

            var results = await Task.Run<IList<AuctionDoc>>(() => query.ToList());

            return results;
        }

        public async Task<AuctionDoc> ReadOneAsync(
            Expression<Func<AuctionDoc, bool>> filter = null)
        {
            return await GetCollection().Find(filter).SingleOrDefaultAsync();
        }

        public void Update(AuctionDoc item)
        {
            if (item == null || string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentException();
            }

            updates.Add(item);
        }

        public void Create(AuctionDoc item)
        {
            throw new NotImplementedException();
        }

        public void Delete(AuctionDoc item)
        {
            throw new NotImplementedException();
        }

        protected IMongoCollection<AuctionDoc> GetCollection()
        {
            return dbContext.GetCollection<AuctionDoc>(COLLECTION_NAME);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (!HasPendingChanges)
            {
                return;
            }
                
            var requests = new List<WriteModel<AuctionDoc>>();
            
            requests.AddRange(updates.Select(doc => new ReplaceOneModel<AuctionDoc>(
                Builders<AuctionDoc>.Filter.Where(a => a.Id.Equals(doc.Id)), doc)));

            var options = new BulkWriteOptions
            {
                IsOrdered = true
            };

            try
            {
                await GetCollection().BulkWriteAsync(requests, options, cancellationToken);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                updates = new List<AuctionDoc>();
            }
        }
    }
}
