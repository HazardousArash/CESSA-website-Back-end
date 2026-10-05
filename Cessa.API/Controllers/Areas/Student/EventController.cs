namespace Cessa.Api.Controllers.Areas.Student
{
    using Cessa.Api.Helpers;
    using Cessa.Application.Features.Events;
    using Cessa.Application.Features.Student.Enrollments;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class UpdateStatusDto
    {
        public int Status { get; set; }
    }

    [Authorize(Roles = "Student")]
    [Route("api/student/[controller]")]
    [ApiExplorerSettings(GroupName = "student")]
    public class EventsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid? userId = Guid.TryParse(userIdStr, out var id) ? id : null;
            var result = await Mediator.Send(new GetEventsQuery(userId));
            return Ok(ApiResult.Success(result, "Events retrieved successfully."));
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollInEventCommand command)
        {
            // SECURITY FIX: Override UserId from JWT claims so students CANNOT enroll on behalf of others
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var safeCommand = command with { UserId = userId };
            await Mediator.Send(safeCommand);
            return Ok(ApiResult.Success("Enrolled successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid? userId = Guid.TryParse(userIdStr, out var uId) ? uId : null;
            var result = await Mediator.Send(new GetEventByIdQuery(id, userId));
            return Ok(ApiResult.Success(result, "Event retrieved successfully."));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            var profile = await Mediator.Send(new Cessa.Application.Features.Student.Profile.GetMyProfileQuery(userId));
            return Ok(ApiResult.Success(profile, "Profile retrieved"));
        }
    }
}
