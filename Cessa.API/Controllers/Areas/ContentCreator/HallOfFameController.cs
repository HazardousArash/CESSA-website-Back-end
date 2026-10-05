namespace Cessa.Api.Controllers.Areas.ContentCreator
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.HallOfFame;
    using Cessa.Api.Helpers;
    using System.Threading.Tasks;

    [Authorize(Roles = "ContentCreator")]
    [Route("api/creator/[controller]")]
    [ApiExplorerSettings(GroupName = "contentcreator")]
    public class HallOfFameController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await Mediator.Send(new GetHallOfFameQuery());
            return Ok(ApiResult.Success(result, "Hall of fame retrieved successfully."));
        }
    }
}