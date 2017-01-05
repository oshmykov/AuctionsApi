using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Objects;
using AuctionsApi.Controllers;
using AuctionsApi.Models.Business.Makers;
using AuctionsApi.Models.Business.Objects;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AuctionsApi.Tests.Controllers
{
    public class AuctionsControllerTests
    {
        [Fact]
        public async void GetAuctions_ReturnsOkWithValues()
        {
            var mockService = new Mock<IAuctionsService>();

            mockService.Setup(
                service => service.GetAuctionsAsync(It.IsAny<AuctionQuery>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuctionMakers.GetAuctions));

            var controller = new AuctionsController(mockService.Object);
            var result = await controller.GetAuctions(new AuctionQuery());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async void GetAuction_ReturnsOkWhenItemExists()
        {
            var mockService = new Mock<IAuctionsService>();
            var auction = AuctionMakers.GetAuctions.SingleOrDefault();

            mockService.Setup(
                service => service.GetAuctionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(auction));

            var controller = new AuctionsController(mockService.Object);

            var result = await controller.GetAuction(string.Empty);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<AuctionSummary>(okResult.Value);
            mockService.Verify();
            Assert.Equal(auction.Id, model.Id);
            Assert.Equal(auction.ExpirationDateTime, model.ExpirationDateTime);
            Assert.Equal(auction.Label, model.Label);
            Assert.Equal(auction.Header, model.Header);
        }

        [Fact]
        public async void GetAuction_ReturnsNotFoundWhenItemNotExists()
        {
            var mockService = new Mock<IAuctionsService>();
            
            mockService.Setup(
                service => service.GetAuctionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<AuctionSummary>(null));

            var controller = new AuctionsController(mockService.Object);

            var result = await controller.GetAuction(string.Empty);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void JoinAuctionsProgram_MatchesBadRequestFromService()
        {
            var mockService = new Mock<IAuctionsService>();
            mockService.Setup(
                service => service.PlaceBidAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(CommandResultMakers.GetPlainCommandResultByType(ResultTypes.BadRequest));

            var controller = new AuctionsController(mockService.Object);

            var result = await controller.PlaceBid(string.Empty, new PlaceBidDetails());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void JoinAuctionsProgram_MatchesNotFoundFromService()
        {
            var mockService = new Mock<IAuctionsService>();
            mockService.Setup(
                service => service.PlaceBidAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(CommandResultMakers.GetPlainCommandResultByType(ResultTypes.NotFound));

            var controller = new AuctionsController(mockService.Object);

            var result = await controller.PlaceBid(string.Empty, new PlaceBidDetails());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void JoinAuctionsProgram_SuccessFromService()
        {
            var mockService = new Mock<IAuctionsService>();
            mockService.Setup(
                service => service.PlaceBidAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(CommandResultMakers.GetPlainCommandResultByType(ResultTypes.Success));

            var controller = new AuctionsController(mockService.Object);

            var result = await controller.PlaceBid(string.Empty, new PlaceBidDetails());

            Assert.IsType<NoContentResult>(result);
        }
    }
}
