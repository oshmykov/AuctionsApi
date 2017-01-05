using AuctionsApi.Models.Data.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using MongoDB.Driver;
using AuctionsApi.Models.Data.Abstract.Mongo;

namespace AuctionsApi.Models.Data.Impl.Mongo
{
    public class ParticipantsMongoRepository : IRepository<ParticipantDoc>
    {
        private const string COLLECTION_NAME = "participants";

        private IMongoDbContext dbContext;

        private IList<ParticipantDoc> inserts;
        private IList<ParticipantDoc> updates;

        public bool HasPendingChanges
        {
            get
            {
                return inserts.Count > 0 || updates.Count > 0;
            }
        }

        public ParticipantsMongoRepository(IMongoDbContext dbContext)
        {
            this.dbContext = dbContext;
            inserts = new List<ParticipantDoc>();
            updates = new List<ParticipantDoc>();
        }

        public async Task<ParticipantDoc> ReadOneAsync(
            Expression<Func<ParticipantDoc, bool>> filter = null)
        {
            return await GetCollection().Find(filter).SingleOrDefaultAsync();
        }

        public void Create(ParticipantDoc item)
        {
            if (item == null || string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentException();
            }

            inserts.Add(item);
        }

        public void Update(ParticipantDoc item)
        {
            if (item == null || string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentException();
            }

            updates.Add(item);
        }

        public void Delete(ParticipantDoc item)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ParticipantDoc>> ReadAsync(
            Expression<Func<ParticipantDoc, bool>> filter = null, 
            string sortBy = null, 
            bool ascending = false, 
            int? skip = default(int?), 
            int? take = default(int?))
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (!HasPendingChanges)
            {
                return;
            }

            var requests = new List<WriteModel<ParticipantDoc>>();
            requests.AddRange(inserts.Select(doc => new InsertOneModel<ParticipantDoc>(doc)));

            requests.AddRange(updates.Select(doc => new ReplaceOneModel<ParticipantDoc>(
                Builders<ParticipantDoc>.Filter.Where(a => a.Id.Equals(doc.Id)), doc)));

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
                inserts = new List<ParticipantDoc>();
                updates = new List<ParticipantDoc>();
            }
        }

        private IMongoCollection<ParticipantDoc> GetCollection()
        {
            return dbContext.GetCollection<ParticipantDoc>(COLLECTION_NAME);
        }
    }
}
