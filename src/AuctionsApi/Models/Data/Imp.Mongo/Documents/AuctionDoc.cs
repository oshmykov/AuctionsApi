using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AuctionsApi.Models.Data.Impl.Mongo.Documents
{
    public class AuctionDoc
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public BidInfoDoc ActiveBid { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }

    public class BidInfoDoc
    {
        public string ParticipantId { get; set; }
        public int BidAmount { get; set; }
        public DateTime PlacedAtUtc { get; set; }
    }
}
