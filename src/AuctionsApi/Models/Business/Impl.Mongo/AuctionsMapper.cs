using AuctionsApi.Business.Models.Objects;
using AuctionsApi.Models.Business.Abstract;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System;
using System.Collections.Generic;

namespace AuctionsApi.Models.Business.Impl.Mongo
{
    public class AuctionsMapper : IMapper<AuctionDoc, AuctionSummary>
    {
        private readonly DateTime utcNow;

        public AuctionsMapper()
        {
            utcNow = DateTime.UtcNow;
        }

        private const int NUMBER_OF_ARGUMENTS_EXPECTED = 1;

        public AuctionSummary Map(AuctionDoc source, params object[] arguments)
        {
            if (arguments.Length != NUMBER_OF_ARGUMENTS_EXPECTED)
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid number of arguments, {0} is expected", NUMBER_OF_ARGUMENTS_EXPECTED));
            }

            var participant = arguments[0] as ParticipantDoc;
            if (participant == null)
            {
                participant = new ParticipantDoc()
                {
                    MyAuctions = new Dictionary<string, int>(),
                    Id = string.Empty
                };
            }

            return DoMapping(source, participant, utcNow);
        }

        private AuctionSummary DoMapping(AuctionDoc auction, ParticipantDoc participant, DateTime nowUtc)
        {
            var hasExpired = auction.ExpiresAtUtc < nowUtc;
            var label = GetLabel(hasExpired,
                participant.MyAuctions.ContainsKey(auction.Id),
                participant.Id.Equals(auction.ActiveBid.ParticipantId));

            return new AuctionSummary
            {
                Id = auction.Id,
                Header = auction.Name,
                ExpirationDateTime = auction.ExpiresAtUtc,
                Label = label,
                BidAmount = auction.ActiveBid.BidAmount
            };
        }

        private AuctionLabels GetLabel(bool hasExpired, bool didBidding, bool isHighBidder)
        {
            if (hasExpired)
            {
                if (didBidding)
                {
                    return isHighBidder ? AuctionLabels.WINNER : AuctionLabels.DID_NOT_WIN;
                }
                else
                {
                    return AuctionLabels.EXPIRED;
                }
            }
            else
            {
                if (didBidding)
                {
                    return isHighBidder ? AuctionLabels.LEADING : AuctionLabels.OUTBID;
                }
                else
                {
                    return AuctionLabels.START_BIDDING;
                }
            }
        }
    }
}
