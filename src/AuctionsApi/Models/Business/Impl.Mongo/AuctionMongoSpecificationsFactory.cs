using AuctionsApi.Business.Models.Objects;
using AuctionsApi.Models.Business.Abstract;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using System;
using System.Collections.Generic;

namespace AuctionsApi.Models.Business.Impl.Mongo
{
    public class AuctionMongoSpecificationsFactory : IAuctionSpecificationsFactory<AuctionDoc>
    {
        public Specification<AuctionDoc> IsActive
        {
            get
            {
                return new IsActiveAuctionSpecification();
            }
        }

        public Specification<AuctionDoc> Not(Specification<AuctionDoc> specification)
        {
            return specification.Invert();
        }

        public Specification<AuctionDoc> DidPlaceBid(string[] myAuctions)
        {
            return new DidPlaceBidAuctionSpecification(myAuctions);
        }

        public Specification<AuctionDoc> IsOwningTheHighestBid(string[] myAuctions, string participantId)
        {
            return new IsOwningTheHighestBidAuctionSpecification(myAuctions, participantId);
        }

        public Specification<AuctionDoc> Temporary(string[] myAuctions, string participantId, string label)
        {
            return new TemporarySpecification(myAuctions, participantId, label);
        }

        public Specification<AuctionDoc> GetSpecification(string labelName, string[] myAuctions, string participantId)
        {
            AuctionLabels label = AuctionLabels.START_BIDDING;
            Enum.TryParse(labelName, out label);

            Specification<AuctionDoc> result = null;

            switch (label)
            {
                case AuctionLabels.LEADING:
                    result = IsActive
                        .And(IsOwningTheHighestBid(myAuctions, participantId));
                    break;
                case AuctionLabels.WINNER:
                    result = IsActive
                        .And(IsOwningTheHighestBid(myAuctions, participantId));
                    break; 
                case AuctionLabels.OUTBID:
                    result = IsActive
                        .And(DidPlaceBid(myAuctions)
                        .Not(IsOwningTheHighestBid(myAuctions, participantId)));
                    break;
                case AuctionLabels.DID_NOT_WIN:
                    result = Not(IsActive)
                        .And(DidPlaceBid(myAuctions)
                        .Not(IsOwningTheHighestBid(myAuctions, participantId)));
                    break;
                default:
                    result = IsActive;
                    break;
            }

            result = Temporary(myAuctions, participantId, labelName);
            return result;
        }
    }
}
