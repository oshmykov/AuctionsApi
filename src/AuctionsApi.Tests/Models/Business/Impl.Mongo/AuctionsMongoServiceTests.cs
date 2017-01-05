using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Impl.Mongo;
using AuctionsApi.Business.Models.Objects;
using AuctionsApi.Models.Business.Abstract;
using AuctionsApi.Models.Business.Impl.Mongo;
using AuctionsApi.Models.Business.Makers;
using AuctionsApi.Models.Business.Objects;
using AuctionsApi.Models.Data.Abstract;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using AuctionsApi.Models.Data.Makers.Mongo;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AuctionsApi.Tests.Models.Business.Impl.Mongo
{
    public class AuctionsMongoServiceTests
    {
        private readonly Mock<IAuctionSpecificationsFactory<AuctionDoc>> mockSpecifications;

        public AuctionsMongoServiceTests()
        {
            mockSpecifications = new Mock<IAuctionSpecificationsFactory<AuctionDoc>>();
            mockSpecifications.Setup(
                spec => spec.GetSpecification(
                    It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>()))
                    .Returns(SpecificationMakers.GetDefault);
        }

        [Fact]
        public async void GetAuctionsAsync_ReturnsItemsAsInvalidParticipant()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();
            var mapper = new AuctionsMapper();

            var activeAuctions = DataAuctionMakers.GetAuctions.Where(
                a => a.ExpiresAtUtc > DateTime.UtcNow);

            mockAuctionsRepo.Setup(
                repo => repo.ReadAsync(
                    It.IsAny<Expression<Func<AuctionDoc, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                    .ReturnsAsync(activeAuctions);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).Returns(Task.FromResult<ParticipantDoc>(null));

            IAuctionSpecificationsFactory<AuctionDoc> specs = new AuctionMongoSpecificationsFactory();

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, mapper);

            var result = await service.GetAuctionsAsync(new AuctionQuery(), string.Empty);
            var enumerable = Assert.IsAssignableFrom<IEnumerable<AuctionSummary>>(result);
            var listResult = enumerable.ToList();
            Assert.Equal(3, listResult.Count);
            Assert.Equal(AuctionLabels.START_BIDDING, listResult[0].Label);
            Assert.Equal(AuctionLabels.START_BIDDING, listResult[1].Label);
        }

        [Theory]
        [InlineData("a-randomly-generated-subject-from-the-identity-provider")]
        [InlineData("a-leading-bidder-participant", 1)]
        [InlineData("an-outbid-participant", null, 1)]
        public async void GetAuctionsAsync_AppliesCorrectLabelForActive(
            string participantId, int? leaderIndex = null, int? outbidIndex = null)
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();
            
            mockAuctionsRepo.Setup(
                repo => repo.ReadAsync(
                    It.IsAny<Expression<Func<AuctionDoc, bool>>>(), 
                    It.IsAny<string>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<int?>(), 
                    It.IsAny<int?>()))
                    .ReturnsAsync(DataAuctionMakers.GetTwoActiveAuctions);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(DataParticipantMakers.GetParticipant(participantId));
            
            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, new AuctionsMapper());

            
            var result = await service.GetAuctionsAsync(new AuctionQuery(), participantId);
            var enumerable = Assert.IsAssignableFrom<IEnumerable<AuctionSummary>>(result);
            var listResult = enumerable.ToList();
            Assert.Equal(2, listResult.Count);
            Assert.Equal(AuctionLabels.START_BIDDING, listResult[0].Label);

            if (leaderIndex.HasValue)
            {
                Assert.Equal(AuctionLabels.LEADING, listResult[leaderIndex.Value].Label);
            }

            if (outbidIndex.HasValue)
            {
                Assert.Equal(AuctionLabels.OUTBID, listResult[outbidIndex.Value].Label);
            }
        }

        [Theory]
        [InlineData("a-randomly-generated-subject-from-the-identity-provider")]
        [InlineData("a-winner-participant", 1)]
        [InlineData("did-not-win-participant", null, 1)]
        public async void GetAuctionsAsync_AppliesCorrectLabelForExpired(
                    string participantId, int? winnerIndex = null, int? didNotWinIndex = null)
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();
            
            mockAuctionsRepo.Setup(
                repo => repo.ReadAsync(
                    It.IsAny<Expression<Func<AuctionDoc, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                    .ReturnsAsync(DataAuctionMakers.GetExpiredAuctions);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(DataParticipantMakers.GetParticipant(participantId));

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, new AuctionsMapper());

            var result = await service.GetAuctionsAsync(new AuctionQuery(), participantId);
            var enumerable = Assert.IsAssignableFrom<IEnumerable<AuctionSummary>>(result);
            var listResult = enumerable.ToList();
            Assert.Equal(2, listResult.Count);
            Assert.Equal(AuctionLabels.EXPIRED, listResult[0].Label);

            if (winnerIndex.HasValue)
            {
                Assert.Equal(AuctionLabels.WINNER, listResult[winnerIndex.Value].Label);
            }
           
            if (didNotWinIndex.HasValue)
            {
                Assert.Equal(AuctionLabels.DID_NOT_WIN, listResult[didNotWinIndex.Value].Label);
            }
        }

        [Fact]
        public async void GetAuctionAsync_ReturnsAuctionById()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();
            
            var auctionDoc = DataAuctionMakers.GetAuction("random-guid-1");

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(null);

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, new AuctionsMapper());

            var result = await service.GetAuctionAsync(string.Empty, string.Empty);
            Assert.IsAssignableFrom<AuctionSummary>(result);
            Assert.Equal(AuctionLabels.START_BIDDING, result.Label);
        }

        [Fact]
        public async void GetAuctionAsync_ReturnsNullWithInvalidAuctionId()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(null);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(null);

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, new AuctionsMapper());

            var result = await service.GetAuctionAsync(string.Empty, string.Empty);
            Assert.Null(result);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsNotFoundWithInvalidParticipantId()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var auctionDoc = DataAuctionMakers.GetAuctions.FirstOrDefault();

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(null);

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 10);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.NotFound, commandResult.ResultType);
            Assert.Equal(1, commandResult.Errors.Count);
            Assert.Equal("Invalid participant Id", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsBadRequestWithInvalidParticipantBalance()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var auctionDoc = DataAuctionMakers.GetAuctions.Where(
                a => a.ExpiresAtUtc > DateTime.UtcNow).FirstOrDefault();

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .ReturnsAsync(DataParticipantMakers.GetParticipant(p => p.Balance < 0));

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 10);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, commandResult.ResultType);
            Assert.Equal(1, commandResult.Errors.Count);
            Assert.Equal("Not enough funds to place a bid", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsNotFoundWithInvalidAuctionId()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(null);

            var anyValidParticipant = DataParticipantMakers.GetParticipant(p => p.Balance > 0);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(anyValidParticipant);

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 10);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.NotFound, commandResult.ResultType);
            Assert.Equal(1, commandResult.Errors.Count);
            Assert.Equal("Invalid auction Id", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsBadRequestWhenNotEnoughFunds()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var highBidAuction = DataAuctionMakers.GetAuctions.Where(
                a => a.ActiveBid.BidAmount > 0)
                .OrderByDescending(p => p.ActiveBid.BidAmount).FirstOrDefault();

            var notEnoughFundsParticipant = DataParticipantMakers.GetParticipant(
                p => p.Balance < highBidAuction.ActiveBid.BidAmount);

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(highBidAuction);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(notEnoughFundsParticipant);

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 10);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, commandResult.ResultType);
            Assert.Equal("Not enough funds to place a bid", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsBadRequestWhenAuctionHasExpired()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var expiredAuction = DataAuctionMakers.GetAuctions.Where(
                a => a.ExpiresAtUtc < DateTime.UtcNow).FirstOrDefault();

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(expiredAuction);

            var validParticipant = DataParticipantMakers.GetParticipant(
                p => p.Balance > expiredAuction.ActiveBid.BidAmount 
                && !p.Id.Equals(expiredAuction.ActiveBid.ParticipantId));

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(validParticipant);

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 100);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, commandResult.ResultType);
            Assert.Equal("Auction has expired", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsBadRequestWhenParticipantAlreadyLeading()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var auctionDoc = DataAuctionMakers.GetAuctions.Where(
                a => a.ExpiresAtUtc > DateTime.UtcNow && a.ActiveBid.BidAmount > 0).FirstOrDefault();

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            var leadingParticipant = DataParticipantMakers.GetParticipant(
                p => p.Id.Equals(auctionDoc.ActiveBid.ParticipantId));

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>())).ReturnsAsync(leadingParticipant);

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 100);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, commandResult.ResultType);
            Assert.Equal("You are a leading bidder already", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsBadRequestWhenParticipantBalanceLessThanCurrent()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var auctionDoc = DataAuctionMakers.GetAuction("random-guid-2");

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .ReturnsAsync(DataParticipantMakers.GetParticipant("an-outbid-participant"));

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 400);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, commandResult.ResultType);
            Assert.Equal(1, commandResult.Errors.Count);
            Assert.Equal("Not enough funds to place a bid", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnsBadRequestBidAmountLessThanCurrent()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var auctionDoc = DataAuctionMakers.GetAuction("random-guid-2");

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .ReturnsAsync(DataParticipantMakers.GetParticipant("did-not-win-participant"));

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 40);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, commandResult.ResultType);
            Assert.Equal(1, commandResult.Errors.Count);
            Assert.Equal("Invalid bid amount", commandResult.Errors[0]);
        }

        [Fact]
        public async void PlaceBidAsync_UpdatesTwoDocumentsWhenFirstTimeBidder()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var auctionDoc = DataAuctionMakers.GetAuction("random-guid-2");

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .ReturnsAsync(DataParticipantMakers.GetParticipant("an-outbid-participant"));

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 60);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, commandResult.ResultType);
        }

        [Fact]
        public async void PlaceBidAsync_ReturnSuccess()
        {
            var mockAuctionsRepo = new Mock<IRepository<AuctionDoc>>();
            var mockParticipantsRepo = new Mock<IRepository<ParticipantDoc>>();

            var auctionDoc = DataAuctionMakers.GetAuction("random-guid-1");

            mockAuctionsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<AuctionDoc, bool>>>())).ReturnsAsync(auctionDoc);

            mockParticipantsRepo.Setup(repo => repo.ReadOneAsync(
                It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .ReturnsAsync(DataParticipantMakers.GetParticipant("an-outbid-participant"));

            var service = new AuctionsMongoService(
                mockAuctionsRepo.Object, mockParticipantsRepo.Object, mockSpecifications.Object, null);

            var result = await service.PlaceBidAsync(string.Empty, string.Empty, 5);
            var commandResult = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.Success, commandResult.ResultType);
        }
    }
}
