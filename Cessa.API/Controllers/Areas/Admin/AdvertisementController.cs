namespace Cessa.Api.Controllers.Areas.Admin
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Advertisements;
    using Cessa.Api.Helpers;
    using System;
    using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class AdvertisementsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(ApiResult.Success(await Mediator.Send(new GetAllAdsQuery()), "Ads retrieved."));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdCommand command) => StatusCode(201, ApiResult.Created(await Mediator.Send(command), "Ad created."));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdCommand command)
        {
            if (id != command.Id) return BadRequest(ApiResult.BadRequest("ID mismatch."));
            await Mediator.Send(command); return Ok(ApiResult.Success("Ad updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteAdCommand(id)); return Ok(ApiResult.Success("Ad deleted."));
        }
    }
}