namespace Cessa.Api.Controllers.Areas.Admin
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Admin.Users;
    using Cessa.Application.Features.Users;
    using Cessa.Api.Helpers;
    using System;
    using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class UsersController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? searchTerm, [FromQuery] int? entryYear)
        {
            var users = await Mediator.Send(new GetUsersQuery(searchTerm, entryYear, IsAdmin: true));
            return Ok(ApiResult.Success(users, "Users retrieved successfully."));
        }

        [HttpGet("explore")]
        public async Task<IActionResult> ExploreUsers([FromQuery] string? searchTerm, [FromQuery] int? entryYear)
        {
            var users = await Mediator.Send(new GetUsersQuery(searchTerm, entryYear, IsAdmin: true));
            return Ok(ApiResult.Success(users, "Users retrieved successfully."));
        }

        // Wipe-out endpoint (supports both POST and DELETE)
        [HttpPost("wipe-out")]
        [HttpDelete("wipe-out")]
        public async Task<IActionResult> WipeOutDeletedUsers()
        {
            var count = await Mediator.Send(new WipeOutDeletedUsersCommand());
            return Ok(ApiResult.Success(new { deletedCount = count }, $"{count} کاربر حذف‌شده با موفقیت برای همیشه پاکسازی شدند."));
        }

        // Added :guid constraint so "wipe-out" will never match this route!
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoveUser(Guid id)
        {
            await Mediator.Send(new RemoveUserCommand(id));
            return Ok(ApiResult.Success("User removed successfully."));
        }

        [HttpPost("{id:guid}/bypass-password")]
        public async Task<IActionResult> BypassPassword(Guid id, [FromBody] string newPassword)
        {
            var result = await Mediator.Send(new BypassUserPasswordCommand(id, newPassword));
            return result
                ? Ok(ApiResult.Success("Password bypassed successfully."))
                : BadRequest(ApiResult.BadRequest("Failed to reset password."));
        }

        [HttpPost("{id:guid}/verify-creator")]
        public async Task<IActionResult> VerifyCreator(Guid id)
        {
            await Mediator.Send(new VerifyContentCreatorCommand(id));
            return Ok(ApiResult.Success("Content Creator verified successfully."));
        }
    }
}
