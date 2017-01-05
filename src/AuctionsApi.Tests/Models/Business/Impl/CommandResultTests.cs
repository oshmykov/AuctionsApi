using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Impl;
using Xunit;

namespace AuctionsApi.Tests.Models.Business.Impl
{
    public class CommandResultTests
    {
        [Fact]
        public void NotFound_ReturnsNotFoundInstance()
        {
            var notFoundErrorMessage = "Not found error message";

            var instance = CommandResult.NotFound(notFoundErrorMessage);

            Assert.IsAssignableFrom<IPlainCommandResult>(instance);
            Assert.Equal(ResultTypes.NotFound, instance.ResultType);
            Assert.Equal(1, instance.Errors.Count);
            Assert.Equal(notFoundErrorMessage, instance.Errors[0]);
        }

        [Fact]
        public void BadRequest_ReturnsBadRequestInstance()
        {
            var badRequestErrorMessage = "Bad request error message";

            var instance = CommandResult.BadRequest(badRequestErrorMessage);

            Assert.IsAssignableFrom<IPlainCommandResult>(instance);
            Assert.Equal(ResultTypes.BadRequest, instance.ResultType);
            Assert.Equal(1, instance.Errors.Count);
            Assert.Equal(badRequestErrorMessage, instance.Errors[0]);
        }

        [Fact]
        public void Success_ReturnsSuccessInstance()
        {
            var instance = CommandResult.Success();

            Assert.IsAssignableFrom<IPlainCommandResult>(instance);
            Assert.Equal(ResultTypes.Success, instance.ResultType);
            Assert.Equal(0, instance.Errors.Count);
        }
    }
}
