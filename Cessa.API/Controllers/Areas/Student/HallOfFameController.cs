namespace Cessa.Api.Controllers.Areas.Student
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.HallOfFame;
    using Cessa.Api.Helpers;
    using System.Threading.Tasks;

    [Authorize(Roles = "Student")]
    [Route("api/student/[controller]")]
    [ApiExplorerSettings(GroupName = "student")]
    public class HallOfFameController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await Mediator.Send(new GetHallOfFameQuery());
            return Ok(ApiResult.Success(result, "Retrieved successfully."));
        }
    }
}