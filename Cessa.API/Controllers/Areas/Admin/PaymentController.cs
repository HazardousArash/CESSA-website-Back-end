namespace Cessa.Api.Controllers.Areas.Admin
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Student.Payments;
    using Cessa.Api.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class PaymentController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await Mediator.Send(new GetMyPaymentsQuery(userId, "Admin"));
            return Ok(ApiResult.Success(result, "Payments retrieved successfully."));
        }
    }
}