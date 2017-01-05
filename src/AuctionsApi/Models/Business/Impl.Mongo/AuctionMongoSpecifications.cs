using AuctionsApi.Models.Business.Abstract;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AuctionsApi.Models.Business.Impl.Mongo
{
    public class IsActiveAuctionSpecification : Specification<AuctionDoc>
    {
        private DateTime utcNow;
        public IsActiveAuctionSpecification()
        {
            utcNow = DateTime.UtcNow;
        }

        public override Expression<Func<AuctionDoc, bool>> ToExpression()
        {
            if (isInverted)
            {
                return auction => auction.ExpiresAtUtc < utcNow;
            }

            return auction => auction.ExpiresAtUtc >= utcNow;
        }
    }

    public class DidPlaceBidAuctionSpecification : Specification<AuctionDoc>
    {
        private readonly ICollection<string> myAuctions;

        public DidPlaceBidAuctionSpecification(ICollection<string> myAuctions)
        {
            this.myAuctions = myAuctions;
        }

        public override Expression<Func<AuctionDoc, bool>> ToExpression()
        {
            if (isInverted)
            {
                return auction => !myAuctions.Contains(auction.Id);
            }

            return auction => myAuctions.Contains(auction.Id);
        }
    }

    public class TemporarySpecification : Specification<AuctionDoc>
    {
        private readonly ICollection<string> myAuctions;
        private readonly string participantId;
        private readonly string label;

        public TemporarySpecification(ICollection<string> myAuctions, string participantId, string label)
        {
            this.myAuctions = myAuctions;
            this.participantId = participantId;
            this.label = label;
        }

        public override Expression<Func<AuctionDoc, bool>> ToExpression()
        {
            var utcNow = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(label))
            {
                if (label.Equals("leading", StringComparison.CurrentCultureIgnoreCase))
                {
                    return a => a.ExpiresAtUtc > utcNow
                        && participantId.Equals(a.ActiveBid.ParticipantId);
                }

                if (label.Equals("winner", StringComparison.CurrentCultureIgnoreCase))
                {
                    return a => a.ExpiresAtUtc < utcNow
                        && participantId.Equals(a.ActiveBid.ParticipantId);
                }
                
                if (label.Equals("outbid", StringComparison.CurrentCultureIgnoreCase))
                {
                    return a => a.ExpiresAtUtc > utcNow
                        && !participantId.Equals(a.ActiveBid.ParticipantId)
                        && myAuctions.Contains(a.Id);
                }

                if (label.Equals("did_not_win", StringComparison.CurrentCultureIgnoreCase))
                {
                    return a => a.ExpiresAtUtc < utcNow
                        && !participantId.Equals(a.ActiveBid.ParticipantId)
                        && myAuctions.Contains(a.Id);
                }
            }

            return a => a.ExpiresAtUtc > utcNow;
        }
    }

    public class IsOwningTheHighestBidAuctionSpecification : Specification<AuctionDoc>
    {
        private readonly ICollection<string> myAuctions;
        private readonly string participantId;

        public IsOwningTheHighestBidAuctionSpecification(ICollection<string> myAuctions, string participantId)
        {
            this.myAuctions = myAuctions;
            this.participantId = participantId;
        }

        public override Expression<Func<AuctionDoc, bool>> ToExpression()
        {
            return auction => myAuctions.Contains(auction.Id)
                && isInverted
                ? !participantId.Equals(auction.ActiveBid.ParticipantId)
                : participantId.Equals(auction.ActiveBid.ParticipantId);
        }
    }
}
