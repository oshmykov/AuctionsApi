using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace AuctionsApi.Business.Models.Objects
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuctionLabels
    {
        [EnumMember(Value = "Start bidding")]
        START_BIDDING,
        [EnumMember(Value = "Did not win")]
        DID_NOT_WIN,
        [EnumMember(Value = "Leading")]
        LEADING,
        [EnumMember(Value = "Outbid")]
        OUTBID,
        [EnumMember(Value = "Winner")]
        WINNER,
        [EnumMember(Value = "Expired")]
        EXPIRED
    }

    public class AuctionSummary
    {
        public string Id { get; set; }
        public string ImageBase64 { get; set; }
        public string Header { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public int BidAmount { get; set; }
        public AuctionLabels Label { get; set; }
    }
}
