namespace Cessa.Api.Controllers.Areas.Admin
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Admin.HallOfFame;
    using Cessa.Application.Features.HallOfFame;
    using Cessa.Api.Helpers;
    using System;
    using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class HallOfFameController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? isAssociation)
        {
            var result = await Mediator.Send(new GetHallOfFameQuery(isAssociation));
            return Ok(ApiResult.Success(result, "Retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddHallOfFameMemberCommand command)
        {
            var id = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, ApiResult.Created(id, "Added successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHallOfFameMemberCommand command)
        {
            if (id != command.Id) return BadRequest(ApiResult.BadRequest("ID mismatch."));
            await Mediator.Send(command);
            return Ok(ApiResult.Success("Member updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteHallOfFameMemberCommand(id));
            return Ok(ApiResult.Success("Member deleted successfully."));
        }
    }
}
