using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Impl.Mongo;
using AuctionsApi.Business.Models.Objects;
using AuctionsApi.Models.Data.Abstract;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;
using AuctionsApi.Models.Data.Makers.Mongo;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AuctionsApi.Tests.Models.Business.Impl.Mongo
{
    public class ParticipantsMongoServiceTests
    {
        private const string exitingParticipantId = "another-randomly-generated-subject-from-the-identity-provider";

        private readonly Mock<IRepository<ParticipantDoc>> mockRepoReturnsParticipant;

        public ParticipantsMongoServiceTests()
        {
            mockRepoReturnsParticipant = new Mock<IRepository<ParticipantDoc>>();

            var participantDoc = DataParticipantMakers.GetParticipant(exitingParticipantId);

            Expression<Func<ParticipantDoc, bool>> expression = p => p.Id.Equals(exitingParticipantId);

            mockRepoReturnsParticipant.Setup(
                repo => repo.ReadOneAsync(
                    It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                    .Callback((Expression<Func<ParticipantDoc, bool>> x) => expression = x)
                .Returns(Task.FromResult(participantDoc));
        }

        [Fact]
        public async void GetParticipantInfo_ReturnsParticipantForValidId()
        {
            var participantDoc = DataParticipantMakers.GetParticipant(exitingParticipantId);

            var service = new ParticipantsMongoService(mockRepoReturnsParticipant.Object);

            var result = await service.GetParticipantInfo(exitingParticipantId);
            
            var participantInfo = Assert.IsType<ParticipantInfo>(result);
            mockRepoReturnsParticipant.Verify();
            Assert.Equal(participantDoc.Id, participantInfo.Id);
            Assert.Equal(participantDoc.UserName, participantInfo.UserName);
            Assert.Equal(participantDoc.Balance, participantInfo.Balance);
        }

        [Fact]
        public async void GetParticipantInfo_ReturnsNullForInvalidId()
        {
            var participantId = "non-existant-id";
            var mockRepository = new Mock<IRepository<ParticipantDoc>>();

            var participantDoc = DataParticipantMakers.GetParticipant(participantId);
            Expression<Func<ParticipantDoc, bool>> expression = p => p.Id.Equals(participantId);

            mockRepository.Setup(
                repo => repo.ReadOneAsync(
                    It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                    .Callback((Expression<Func<ParticipantDoc, bool>> x) => expression = x)
                .Returns(Task.FromResult(participantDoc));

            var service = new ParticipantsMongoService(mockRepository.Object);

            var result = await service.GetParticipantInfo(participantId);

            Assert.Null(result);
        }

        [Fact]
        public async void JoinAuctionsProgram_ReturnsErrorForAlreadyRegisteredUsers()
        {
            var service = new ParticipantsMongoService(mockRepoReturnsParticipant.Object);

            var result = await service.JoinAuctionsProgram(exitingParticipantId, "A Name", 430);

            var badRequest = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, badRequest.ResultType);
            Assert.Equal(1, badRequest.Errors.Count);
            Assert.Equal("Participant has joined the Program already", badRequest.Errors[0]);
        }

        [Fact]
        public async void JoinAuctionsProgram_ReturnsErrorForNegativeBalance()
        {
            var mockRepository = new Mock<IRepository<ParticipantDoc>>();

            mockRepository.Setup(
                repo => repo.ReadOneAsync(
                    It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .Returns(Task.FromResult<ParticipantDoc>(null));

            var service = new ParticipantsMongoService(mockRepository.Object);

            var result = await service.JoinAuctionsProgram("a-new-random-id", "A Name", -10);

            var badRequest = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, badRequest.ResultType);
            Assert.Equal(1, badRequest.Errors.Count);
            Assert.Equal("Invalid balance to join the Program", badRequest.Errors[0]);
        }

        [Fact]
        public async void JoinAuctionsProgram_ReturnsExceptionMessageWhenRepositoryFails()
        {
            var exceptionMessage = "oops, sorry";

            var mockRepository = new Mock<IRepository<ParticipantDoc>>();

            mockRepository.Setup(
                repo => repo.ReadOneAsync(
                    It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .Returns(Task.FromResult<ParticipantDoc>(null));

            mockRepository.Setup(
                repo => repo.Create(It.IsAny<ParticipantDoc>()));

            mockRepository.Setup(
                repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>())
                ).Throws(new Exception(exceptionMessage));

            var service = new ParticipantsMongoService(mockRepository.Object);

            var result = await service.JoinAuctionsProgram("a-new-random-id", "A Name", 10);

            var badRequest = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.BadRequest, badRequest.ResultType);
            Assert.Equal(1, badRequest.Errors.Count);
            Assert.Equal(exceptionMessage, badRequest.Errors[0]);
        }

        [Fact]
        public async void JoinAuctionsProgram_ReturnsSuccess()
        {
            var mockRepository = new Mock<IRepository<ParticipantDoc>>();

            mockRepository.Setup(
                repo => repo.ReadOneAsync(
                    It.IsAny<Expression<Func<ParticipantDoc, bool>>>()))
                .Returns(Task.FromResult<ParticipantDoc>(null));

            mockRepository.Setup(
                repo => repo.Create(It.IsAny<ParticipantDoc>()));

            var service = new ParticipantsMongoService(mockRepository.Object);

            var result = await service.JoinAuctionsProgram("a-new-random-id", "A Name", 10);

            var success = Assert.IsAssignableFrom<IPlainCommandResult>(result);
            Assert.Equal(ResultTypes.Success, success.ResultType);
            Assert.Equal(0, success.Errors.Count);
        }
    }
}
