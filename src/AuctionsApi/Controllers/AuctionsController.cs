using AuctionsApi.Helpers;
using AuctionsApi.Business.Models.Abstract;
using Microsoft.AspNetCore.Mvc;
using AuctionsApi.Models.Business.Objects;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace AuctionsApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AuctionsController : Controller
    {
        private readonly IAuctionsService auctionsService;

        public AuctionsController(IAuctionsService auctionsService)
        {
            this.auctionsService = auctionsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuctions([FromQuery] AuctionQuery query)
        {
            var result = await auctionsService.GetAuctionsAsync(query, User.GetSubject());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuction(string id)
        {
            var result = await auctionsService.GetAuctionAsync(id, User.GetSubject());
            if (result != null)
            {
                return new OkObjectResult(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/placeBid")]
        public async Task<IActionResult> PlaceBid(string id, [FromBody] PlaceBidDetails model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            var result = await auctionsService.PlaceBidAsync(id, User.GetSubject(), model.BidAmount);

            switch (result.ResultType)
            {
                case ResultTypes.NotFound:
                    return NotFound(result.Errors);
                case ResultTypes.BadRequest:
                    return BadRequest(result.Errors);
                default:
                    return NoContent();
            }
        }
    }
}
