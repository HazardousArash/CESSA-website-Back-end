namespace Cessa.Api.Controllers.Areas.ContentCreator
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Cessa.Application.Features.Student.Profile;
    using Cessa.Api.Helpers;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [Authorize(Roles = "ContentCreator")]
    [Route("api/creator/[controller]")]
    [ApiExplorerSettings(GroupName = "contentcreator")]
    public class ProfileController : ApiControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = await Mediator.Send(new GetMyProfileQuery(userId));
            return Ok(ApiResult.Success(profile, "Profile retrieved"));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != command.UserId.ToString()) return StatusCode(StatusCodes.Status403Forbidden, ApiResult.Forbidden("You can only modify your own profile."));

            await Mediator.Send(command);
            return Ok(ApiResult.Success("Profile updated successfully."));
        }
    }
}