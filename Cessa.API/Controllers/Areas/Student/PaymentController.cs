namespace Cessa.Api.Controllers.Areas.Student
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Student.Payments;
    using Cessa.Api.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [Authorize(Roles = "Student")]
    [Route("api/student/[controller]")]
    [ApiExplorerSettings(GroupName = "student")]
    public class PaymentController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await Mediator.Send(new GetMyPaymentsQuery(userId, "Student"));
            return Ok(ApiResult.Success(result, "Payments retrieved successfully."));
        }
    }
}