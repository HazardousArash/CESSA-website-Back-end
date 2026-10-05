namespace Cessa.Api.Controllers.Areas.Public
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Posts;
    using Cessa.Application.Features.Events;
    using Cessa.Application.Features.HallOfFame;
    using Cessa.Api.Helpers;
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    [AllowAnonymous]
    [Route("api/public/[controller]")]
    [ApiExplorerSettings(GroupName = "public")]
    public class FeedController : ApiControllerBase
    {
        [HttpGet("posts")]
        public async Task<IActionResult> GetRecentPosts() => Ok(ApiResult.Success(await Mediator.Send(new GetRecentPostsQuery(null)), "Recent posts retrieved successfully."));

        [HttpGet("posts/{id:guid}")]
        public async Task<IActionResult> GetPostById(Guid id) => Ok(ApiResult.Success(await Mediator.Send(new GetPostByIdQuery(id, null)), "Post retrieved successfully."));

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents() => Ok(ApiResult.Success(await Mediator.Send(new GetEventsQuery(null)), "Events retrieved successfully."));

        [HttpGet("events/{id:guid}")]
        public async Task<IActionResult> GetEventById(Guid id) => Ok(ApiResult.Success(await Mediator.Send(new GetEventByIdQuery(id, null)), "Event retrieved successfully."));

        [HttpGet("halloffame")]
        public async Task<IActionResult> GetHallOfFame([FromQuery] bool? isAssociation)
        {
            var result = await Mediator.Send(new GetHallOfFameQuery(isAssociation));
            return Ok(ApiResult.Success(result, "Hall of fame retrieved successfully."));
        }

        [HttpGet("advertisements")]
        public async Task<IActionResult> GetActiveAdvertisements() => Ok(ApiResult.Success(await Mediator.Send(new Cessa.Application.Features.Advertisements.GetActiveAdsQuery()), "Ads retrieved successfully."));

        [HttpPost("verify-secret")]
        public IActionResult VerifySecretDoor([FromBody] string secret, [FromServices] IConfiguration config)
        {
            var expected = config["SecretDoor"] ?? "arash";
            if (secret == expected) return Ok(ApiResult.Success("Unlocked"));
            return BadRequest(ApiResult.BadRequest("Access Denied"));
        }
    }
}
