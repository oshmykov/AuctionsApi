using AuctionsApi.Business.Models.Objects;
using System;
using System.Collections.Generic;

namespace AuctionsApi.Models.Business.Makers
{
    public static class AuctionMakers
    {
        public static readonly IList<AuctionSummary> auctions = new List<AuctionSummary>
        {
            new AuctionSummary
            {
                Id = "random-guid-1",
                BidAmount = 0,
                ExpirationDateTime = DateTime.UtcNow.AddDays(30),
                Header = "Christmas Gift Rotation Color Changing Aurora Projection LED Night Light Lamp",
                Label = AuctionLabels.START_BIDDING
            },

        };

        public static IEnumerable<AuctionSummary> GetAuctions
        {
            get
            {
                return auctions;
            }
        }
    }
}
