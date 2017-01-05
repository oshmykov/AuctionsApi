using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionsApi.Models.Data.Impl.Mongo.Documents
{
    public class ParticipantDoc
    {
        [BsonId]
        public string Id { get; set; }
        public string UserName { get; set; }
        public int Balance { get; set; }
        public IDictionary<string, int> MyAuctions { get; set; }
    }
}
