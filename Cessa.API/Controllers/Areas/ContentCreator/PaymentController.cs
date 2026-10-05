namespace Cessa.Api.Controllers.Areas.ContentCreator
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Student.Payments;
    using Cessa.Api.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [Authorize(Roles = "ContentCreator")]
    [Route("api/creator/[controller]")]
    [ApiExplorerSettings(GroupName = "contentcreator")]
    public class PaymentController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await Mediator.Send(new GetMyPaymentsQuery(userId, "ContentCreator"));
            return Ok(ApiResult.Success(result, "Payments retrieved successfully."));
        }
    }
}