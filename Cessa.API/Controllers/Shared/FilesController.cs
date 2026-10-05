namespace Cessa.Api.Controllers.Shared
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Cessa.Application.Features.Files;
    using Cessa.Api.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [Authorize]
    [Route("api/[controller]")]
    public class FilesController : ApiControllerBase
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 Megabytes

        [HttpPost("upload")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0) 
                return BadRequest(ApiResult.BadRequest("File is missing."));

            if (file.Length > MaxFileSize)
                return BadRequest(ApiResult.BadRequest("حجم فایل نباید بیش از ۱۰ مگابایت باشد."));

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

            using var stream = file.OpenReadStream();
            var command = new UploadFileCommand(stream, file.FileName, file.ContentType, userId, role);
            var fileId = await Mediator.Send(command);

            return Ok(ApiResult.Success(fileId, "File uploaded successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

            await Mediator.Send(new Cessa.Application.Features.Files.DeleteFileCommand(id, userId, role));
            return Ok(ApiResult.Success("File deleted successfully."));
        }
    }
}
