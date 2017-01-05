using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuctionsApi.Models.Data.Abstract;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using AuctionsApi.Models.Business.Objects;
using AuctionsApi.Models.Business.Abstract;
using System.Linq;
using AuctionsApi.Models.Data.Impl;

namespace AuctionsApi.Business.Models.Impl.Mongo
{
    public class AuctionsMongoService : IAuctionsService
    {
        private const string INVALID_PARTICIPANT_ID = "Invalid participant Id";
        private const string NOT_ENOUGH_FUNDS = "Not enough funds to place a bid";
        private const string INVALID_AUCTION_ID = "Invalid auction Id";
        private const string INVALID_TIMESTAMP = "Bid rejected due to invalid timestamp";
        private const string AUCTION_HAS_EXPIRED = "Auction has expired";
        private const string RAISING_IS_FORBIDDEN = "You are a leading bidder already";
        private const string INVALID_BID_AMOUNT = "Invalid bid amount";

        private readonly IRepository<AuctionDoc> auctionsRepository;
        private readonly IRepository<ParticipantDoc> participantsRepository;
        private readonly IAuctionSpecificationsFactory<AuctionDoc> auctionSpecs;
        private readonly IMapper<AuctionDoc, AuctionSummary> auctionsMapper;

        public AuctionsMongoService(
            IRepository<AuctionDoc> auctionsRepository, 
            IRepository<ParticipantDoc> participantsRepository,
            IAuctionSpecificationsFactory<AuctionDoc> auctionSpecs,
            IMapper<AuctionDoc, AuctionSummary> auctionsMapper)
        {
            this.auctionsRepository = auctionsRepository;
            this.participantsRepository = participantsRepository;
            this.auctionSpecs = auctionSpecs;
            this.auctionsMapper = auctionsMapper;
        }

        public async Task<IEnumerable<AuctionSummary>> GetAuctionsAsync(AuctionQuery queryParameters, string participantId)
        {
            var participant = await participantsRepository.ReadOneAsync(p => p.Id.Equals(participantId));
            var participantAuctions = new string[0];
            if (participant != null)
            {
                participantAuctions = participant.MyAuctions.Keys.ToArray();
            }

            var filter = auctionSpecs.GetSpecification(
                queryParameters.Label, participantAuctions, participantId).ToExpression();

            var auctions = await auctionsRepository.ReadAsync(filter,
                queryParameters.SortBy, queryParameters.Ascending, queryParameters.Skip, queryParameters.Take);

            return auctions.Select(auction => auctionsMapper.Map(auction, participant));
        }

        public async Task<AuctionSummary> GetAuctionAsync(string auctionId, string participantId)
        {
            var participant = await participantsRepository.ReadOneAsync(p => p.Id.Equals(participantId));

            var auction = await auctionsRepository.ReadOneAsync(a => a.Id.Equals(auctionId));

            if (auction == null) {
                return null;
            }

            return auctionsMapper.Map(auction, participant);
        }

        public async Task<IPlainCommandResult> PlaceBidAsync(string auctionId, string participantId, int bidAmount)
        {
            var participant = await participantsRepository.ReadOneAsync(p => p.Id.Equals(participantId));

            if (participant == null)
            {
                return CommandResult.NotFound(INVALID_PARTICIPANT_ID);
            }

            if (participant.Balance <= 0 || bidAmount > participant.Balance)
            {
                return CommandResult.BadRequest(NOT_ENOUGH_FUNDS);
            }

            var auction = await auctionsRepository.ReadOneAsync(a => a.Id.Equals(auctionId));
            if (auction == null)
            {
                return CommandResult.NotFound(INVALID_AUCTION_ID);
            }

            if (auction.ActiveBid.BidAmount >= participant.Balance)
            {
                return CommandResult.BadRequest(NOT_ENOUGH_FUNDS);
            }

            DateTime utcNow = DateTime.UtcNow;

            if (auction.ExpiresAtUtc < utcNow)
            {
                return CommandResult.BadRequest(AUCTION_HAS_EXPIRED);
            }

            if (utcNow <= auction.ActiveBid.PlacedAtUtc)
            {
                return CommandResult.BadRequest(INVALID_TIMESTAMP);
            }

            if (auction.ActiveBid.ParticipantId == participant.Id)
            {
                return CommandResult.BadRequest(RAISING_IS_FORBIDDEN);
            }

            if (bidAmount <= auction.ActiveBid.BidAmount)
            {
                return CommandResult.BadRequest(INVALID_BID_AMOUNT);
            }

            if (LeadingBidderExists(auction))
            {
                UpdateExistingBidder(auction);
            }

            UpdateParticipant(auction.Id, participant, bidAmount);

            UpdateAuction(auction, participantId, bidAmount, utcNow);

            try {
                await participantsRepository.SaveChangesAsync();
                await auctionsRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return CommandResult.BadRequest(ex.Message);
            }

            return CommandResult.Success();
        }

        private bool LeadingBidderExists(AuctionDoc auction)
        {
            return !string.IsNullOrEmpty(auction.ActiveBid.ParticipantId);
        }

        private async void UpdateExistingBidder(AuctionDoc auction)
        {
            var leadingBidder = await participantsRepository.ReadOneAsync(p => p.Id.Equals(auction.ActiveBid.ParticipantId));
            if (leadingBidder != null)
            {
                var hisAuctions = new Dictionary<string, int>(leadingBidder.MyAuctions);
                if (hisAuctions.ContainsKey(auction.Id))
                {
                    hisAuctions[auction.Id] = 0;
                }
                else
                {
                    throw new DataIntegrityViolationException(
                        string.Format(
                            "Participant {0} does not contain the auction key {1}", leadingBidder.Id, auction.Id));
                }

                participantsRepository.Update(new ParticipantDoc
                {
                    Id = leadingBidder.Id,
                    UserName = leadingBidder.UserName,
                    Balance = leadingBidder.Balance + auction.ActiveBid.BidAmount,
                    MyAuctions = hisAuctions
                });
            }
            else
            {
                throw new DataIntegrityViolationException(
                    string.Format(
                        "Participant {0} does not exist, but auction {1} is expecting", leadingBidder.Id, auction.Id));
            }
        }

        private void UpdateParticipant(string auctionId, ParticipantDoc participant, int bidAmount)
        {
            var participantAuctions = new Dictionary<string, int>(participant.MyAuctions);
            if (participantAuctions.ContainsKey(auctionId))
            {
                participantAuctions[auctionId] = bidAmount;
            }
            else
            {
                participantAuctions.Add(auctionId, bidAmount);
            }

            participantsRepository.Update(new ParticipantDoc
            {
                Id = participant.Id,
                UserName = participant.UserName,
                Balance = participant.Balance - bidAmount,
                MyAuctions = participantAuctions
            });
        }

        private void UpdateAuction(AuctionDoc auction, string participantId, int bidAmount, DateTime utcNow)
        {
            auctionsRepository.Update(new AuctionDoc
            {
                Id = auction.Id,
                Name = auction.Name,
                ExpiresAtUtc = auction.ExpiresAtUtc,
                ActiveBid = new BidInfoDoc
                {
                    BidAmount = bidAmount,
                    ParticipantId = participantId,
                    PlacedAtUtc = utcNow
                }
            });
        }
    }
}
