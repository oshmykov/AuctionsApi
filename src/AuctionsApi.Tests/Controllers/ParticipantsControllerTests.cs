using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Objects;
using AuctionsApi.Controllers;
using AuctionsApi.Models.Business.Makers;
using AuctionsApi.Models.Business.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace AuctionsApi.Tests.Controllers
{
    public class ParticipantsControllerTests
    {
        private const string participantId = "a-randomly-generated-subject-from-the-identity-provider";

        private Mock<ClaimsPrincipal> mockUser;
        
        public ParticipantsControllerTests()
        {
            mockUser = new Mock<ClaimsPrincipal>();
            mockUser.Setup(user => user.Claims).Returns(ParticipantMakers.GetUserClaims(participantId));
        }

        [Fact]
        public async void GetMyInfo_GetsResultFromServiceByUserId()
        {
            var mockService = new Mock<IParticipantsService>();

            mockService.Setup(
                service => service.GetParticipantInfo(participantId))
                .Returns(Task.FromResult(ParticipantMakers.GetInfo(participantId)));

            var controller = new ParticipantsController(mockService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() { User = mockUser.Object }
            };

            var participant = ParticipantMakers.GetInfo(participantId);

            var result = controller.GetMyInfo();

            var actionResult = await Assert.IsType<Task<IActionResult>>(result);
            var okResult = Assert.IsAssignableFrom<OkObjectResult>(actionResult);
            var model = Assert.IsType<ParticipantInfo>(okResult.Value);
            mockService.Verify();
            Assert.Equal(participant.Id, model.Id);
            Assert.Equal(participant.UserName, model.UserName);
            Assert.Equal(participant.Balance, model.Balance);
        }

        [Fact]
        public async void JoinAuctionsProgram_MatchesBadRequestFromService()
        {
            var mockService = new Mock<IParticipantsService>();
            mockService.Setup(
                service => service.JoinAuctionsProgram(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(CommandResultMakers.GetPlainCommandResultByType(ResultTypes.BadRequest));

            var controller = new ParticipantsController(mockService.Object);

            var result = await controller.JoinAuctionsProgram(new ParticipantJoinInfo());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void JoinAuctionsProgram_MatchesNotFoundFromService()
        {
            var mockService = new Mock<IParticipantsService>();
            mockService.Setup(
                service => service.JoinAuctionsProgram(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(CommandResultMakers.GetPlainCommandResultByType(ResultTypes.NotFound));

            var controller = new ParticipantsController(mockService.Object);

            var result = await controller.JoinAuctionsProgram(new ParticipantJoinInfo());

            Assert.IsType<NotFoundObjectResult>(result);
        }
        
        [Fact]
        public async void JoinAuctionsProgram_SuccessFromService()
        {
            var mockService = new Mock<IParticipantsService>();
            mockService.Setup(
                service => service.JoinAuctionsProgram(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(CommandResultMakers.GetPlainCommandResultByType(ResultTypes.Success));

            var controller = new ParticipantsController(mockService.Object);

            var result = await controller.JoinAuctionsProgram(new ParticipantJoinInfo());

            Assert.IsType<NoContentResult>(result);
        }
    }
}
