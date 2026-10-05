namespace Cessa.Api.Controllers.Areas.ContentCreator
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.ContentCreator.Events;
    using Cessa.Application.Features.Events;
    using Cessa.Application.Features.Student.Enrollments;
    using Cessa.Api.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class UpdateStatusDto
    {
        public int Status { get; set; }
    }

    [Authorize(Roles = "ContentCreator")]
    [Route("api/creator/[controller]")]
    [ApiExplorerSettings(GroupName = "contentcreator")]
    public class EventsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = string.IsNullOrEmpty(userIdString) ? null : Guid.Parse(userIdString);
            var result = await Mediator.Send(new GetEventsQuery(userId));
            return Ok(ApiResult.Success(result, "Events retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command)
        {
            var creatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var newCommand = command with { CreatorId = creatorId };
            var result = await Mediator.Send(newCommand);
            return StatusCode(StatusCodes.Status201Created, ApiResult.Created(result, "Event created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventCommand command)
        {
            if (id != command.Id) return BadRequest(ApiResult.BadRequest("ID mismatch."));
            var creatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var newCommand = command with { CreatorId = creatorId };
            await Mediator.Send(newCommand);
            return Ok(ApiResult.Success("Event updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var creatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await Mediator.Send(new DeleteEventCommand(id, creatorId));
            return Ok(ApiResult.Success("Event deleted successfully."));
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollInEventCommand command)
        {
            await Mediator.Send(command);
            return Ok(ApiResult.Success("Enrolled successfully."));
        }

        [HttpGet("{eventId}/enrollments")]
        public async Task<IActionResult> GetEnrollments(Guid eventId)
        {
            var creatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await Mediator.Send(new GetEventEnrollmentsQuery(eventId, creatorId, IsAdmin: false));
            return Ok(ApiResult.Success(result, "Enrollments retrieved successfully."));
        }

        [HttpPatch("{eventId}/enrollments/{enrollmentId}/status")]
        public async Task<IActionResult> UpdateEnrollmentStatus(Guid eventId, Guid enrollmentId, [FromBody] UpdateStatusDto request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool isAdmin = User.IsInRole("Admin");
            var enumStatus = (Cessa.Domain.Enums.EnrollmentStatus)request.Status;
            await Mediator.Send(new UpdateEnrollmentStatusCommand(eventId, enrollmentId, enumStatus, userId, isAdmin));
            return Ok(ApiResult.Success("Enrollment status updated."));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = string.IsNullOrEmpty(userIdString) ? null : Guid.Parse(userIdString);
            var result = await Mediator.Send(new GetEventByIdQuery(id, userId));
            return Ok(ApiResult.Success(result, "Event retrieved successfully."));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = await Mediator.Send(new Cessa.Application.Features.Student.Profile.GetMyProfileQuery(userId));
            return Ok(ApiResult.Success(profile, "Profile retrieved"));
        }
    }
}