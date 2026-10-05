namespace Cessa.Api.Controllers.Areas.Admin
{
    using Cessa.Api.Helpers;
    using Cessa.Application.Features.Admin.Events;
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

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class EventsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid? userId = Guid.TryParse(userIdString, out var id) ? id : null;

            var result = await Mediator.Send(new GetEventsQuery(userId));
            return Ok(ApiResult.Success(result, "Events retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command)
        {
            var creatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var creatorId = Guid.TryParse(creatorIdStr, out var id) ? id : command.CreatorId;
            var newCommand = command with { CreatorId = creatorId };
            var result = await Mediator.Send(newCommand);
            return StatusCode(StatusCodes.Status201Created, ApiResult.Created(result, "Event created successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventCommand command)
        {
            if (id != command.Id) return BadRequest(ApiResult.BadRequest("ID mismatch in route and body."));
            await Mediator.Send(command);
            return Ok(ApiResult.Success("Event updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            await Mediator.Send(new DeleteEventCommand(id));
            return Ok(ApiResult.Success("Event deleted successfully."));
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollInEventCommand command)
        {
            await Mediator.Send(command);
            return Ok(ApiResult.Success("Enrolled successfully."));
        }

        [HttpGet("{eventId:guid}/enrollments")]
        public async Task<IActionResult> GetEnrollments(Guid eventId)
        {
            var creatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(creatorIdStr, out var creatorId)) return Unauthorized();
            var result = await Mediator.Send(new Cessa.Application.Features.Events.GetEventEnrollmentsQuery(eventId, creatorId, IsAdmin: true));
            return Ok(ApiResult.Success(result, "Enrollments retrieved successfully."));
        }

        [HttpPatch("{eventId:guid}/enrollments/{enrollmentId:guid}/status")]
        public async Task<IActionResult> UpdateEnrollmentStatus(Guid eventId, Guid enrollmentId, [FromBody] UpdateStatusDto request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            bool isAdmin = User.IsInRole("Admin");
            var enumStatus = (Cessa.Domain.Enums.EnrollmentStatus)request.Status;
            await Mediator.Send(new UpdateEnrollmentStatusCommand(eventId, enrollmentId, enumStatus, userId, isAdmin));
            return Ok(ApiResult.Success("Enrollment status updated."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid? userId = Guid.TryParse(userIdString, out var uId) ? uId : null;
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
