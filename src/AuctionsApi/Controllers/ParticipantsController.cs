using AuctionsApi.Helpers;
using AuctionsApi.Business.Models.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AuctionsApi.Models.Business.Objects;

namespace AuctionsApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ParticipantsController : Controller
    {
        private const string NOT_FOUND = "You have not joined the Auctions Program yet";

        private readonly IParticipantsService participantsService;

        public ParticipantsController(IParticipantsService participantsService)
        {
            this.participantsService = participantsService;
        }

        [HttpGet("GetMyInfo")]
        public async Task<IActionResult> GetMyInfo()
        {
            var sub = User.GetSubject();

            var result = await participantsService.GetParticipantInfo(sub);
            
            if (result != null)
            {
                return new OkObjectResult(result);
            }
            else
            {
                return NotFound(string.Format(NOT_FOUND));
            }
        }

        [HttpPost("Join")]
        public async Task<IActionResult> JoinAuctionsProgram([FromBody] ParticipantJoinInfo model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var sub = User.GetSubject();
            var username = User.GetUserName();

            var result = await participantsService.JoinAuctionsProgram(sub, username, model.Balance);
            
            switch (result.ResultType)
            {
                case ResultTypes.BadRequest:
                    return BadRequest(result.Errors);
                case ResultTypes.NotFound:
                    return NotFound(result.Errors);
                default:
                    return NoContent();
            }
        }
    }
}
