using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System.Linq.Expressions;
using MongoDB.Driver;
using AuctionsApi.Models.Data.Abstract.Mongo;
using System.Linq;

namespace AuctionsApi.Models.Data.Impl.Mongo
{
    public class AuctionsMongoDemoRepository : AuctionsMongoRepository
    {
        private static readonly string[] AUCTION_NAMES = {
            "Renee Petrillo - A Sail of Two Idiots",
            "Bill Bryson - A Walk in the Woods",
            "Gregory David - Shantaram",
            "Bill Bryson - Bill Bryson's African Diary",
            "Maarten Troost - Getting Stoned With Savages",
            "Anthony Bourdain - Kitchen Confidential",
            "Bill Bryson - Lost Continent",
            "Richard Ford - Let Me Be Frank With You",
            "Gregory David - The Mountain Shadow",
            "Jack London - Hearts of Three",
            "Arthur Hailey - Airport",
            "William Shakespeare - Hamlet"
        };

        private const int ACTIVE_AUCTION_THRESHOLD = 5;
        private const int MIN_OFFSET_IN_MINUTES = 30;
        private const int MAX_OFFSET_IN_MINUTES = 45;

        public AuctionsMongoDemoRepository(IMongoDbContext dbContext) : base(dbContext)
        {
        }

        public async override Task<IEnumerable<AuctionDoc>> ReadAsync(
            Expression<Func<AuctionDoc, bool>> filter = null,
            string sortBy = EXPIRATION,
            bool ascending = true,
            int? skipAmount = default(int?),
            int? limitAmount = default(int?))
        {
            await RemoveExpiredAuctionsWithoutBids();

            var activeAuctions = await base.ReadAsync(a => a.ExpiresAtUtc > DateTime.UtcNow);
            await MakeSomeDemoAuctions(ACTIVE_AUCTION_THRESHOLD - activeAuctions.ToList().Count);

            return await base.ReadAsync(filter, sortBy, ascending, skipAmount, limitAmount);
        }

        private async Task RemoveExpiredAuctionsWithoutBids()
        {
            await GetCollection().DeleteManyAsync(a => a.ExpiresAtUtc < DateTime.UtcNow && a.ActiveBid.BidAmount == 0);
        }

        private async Task MakeSomeDemoAuctions(int count)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            
            for (var i = 0; i <= count; i++)
            {
                try
                {
                    await GetCollection().InsertOneAsync(new AuctionDoc
                    {
                        Name = AUCTION_NAMES[random.Next(AUCTION_NAMES.Length)],
                        ActiveBid = new BidInfoDoc(),
                        ExpiresAtUtc = DateTime.UtcNow.AddMinutes(random.Next(MIN_OFFSET_IN_MINUTES, MAX_OFFSET_IN_MINUTES))
                    });
                }
                catch(Exception ex)
                {
                    string exMEsage = ex.Message;
                }
            }
        }
    }
}
