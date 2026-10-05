namespace Cessa.Api.Controllers.Areas.Admin
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Admin.Logs;
    using Cessa.Api.Helpers;
    using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class LogsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSystemLogs()
        {
            var logs = await Mediator.Send(new GetSystemLogsQuery());
            return Ok(ApiResult.Success(logs, "System logs retrieved successfully."));
        }
    }
}
