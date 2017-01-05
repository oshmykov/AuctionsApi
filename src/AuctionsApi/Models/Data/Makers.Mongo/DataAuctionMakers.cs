using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuctionsApi.Models.Data.Makers.Mongo
{
    public class DataAuctionMakers
    {
        private static readonly IList<AuctionDoc> auctions = new List<AuctionDoc>
        {
            new AuctionDoc
            {
                Id = "random-guid-1",
                Name = "Christmas Gift Rotation Color Changing Aurora Projection LED Night Light Lamp",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
                ActiveBid = new BidInfoDoc
                {
                    BidAmount = 0
                }
            },
            new AuctionDoc
            {
                Id = "random-guid-2",
                Name = "Constellation Night Light, SCOPOW Star Sky with LED",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(60),
                ActiveBid = new BidInfoDoc
                {
                    BidAmount = 50,
                    ParticipantId = "a-leading-bidder-participant",
                    PlacedAtUtc = DateTime.UtcNow.AddDays(-5)
                }
            },
            new AuctionDoc
            {
                Id = "random-guid-3",
                Name = "Supertech Multicolor Ocean Wave Projector Night Light Lamp",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-3),
                ActiveBid = new BidInfoDoc
                {
                    BidAmount = 250,
                    ParticipantId = "a-winner-participant",
                    PlacedAtUtc = DateTime.UtcNow.AddDays(-4)
                }
            },
            new AuctionDoc
            {
                Id = "random-guid-4",
                Name = "Vacmaster Exhaust Filter",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-30),
                ActiveBid = new BidInfoDoc
                {
                    BidAmount = 0
                }
            },
            new AuctionDoc
            {
                Id = "random-guid-5-high-bid",
                Name = "Vacmaster Exhaust Filter",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
                ActiveBid = new BidInfoDoc
                {
                    BidAmount = 99999
                }
            }
        };

        
        public static IEnumerable<AuctionDoc> GetTwoActiveAuctions
        {
            get
            {
                return new List<AuctionDoc>()
                {
                    auctions[0], auctions[1]
                };
            }
        }

        public static IEnumerable<AuctionDoc> GetExpiredAuctions
        {
            get
            {
                return new List<AuctionDoc>()
                {
                    auctions[3], auctions[2]
                };
            }
        }

        public static AuctionDoc GetAuction(string id)
        {
            return auctions.Where(auction => auction.Id.Equals(id)).SingleOrDefault();
        }

        public static IEnumerable<AuctionDoc> GetAuctions
        {
            get
            {
                return auctions;
            }
        }
    }
}
