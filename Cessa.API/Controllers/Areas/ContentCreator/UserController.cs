//namespace Cessa.Api.Controllers.Areas.ContentCreator
//{
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.AspNetCore.Authorization;
//    using Cessa.Application.Features.Users;
//    using Cessa.Api.Helpers;
//    using System.Threading.Tasks;

//    [Authorize(Roles = "ContentCreator")]
//    [Route("api/creator/[controller]")]
//    [ApiExplorerSettings(GroupName = "contentcreator")]
//    public class UserController : ApiControllerBase
//    {
//        [HttpGet("explore")]
//        public async Task<IActionResult> ExploreUsers([FromQuery] string? searchTerm, [FromQuery] int? entryYear)
//        {
//            var users = await Mediator.Send(new GetUsersQuery(searchTerm, entryYear));
//            return Ok(ApiResult.Success(users, "Users retrieved successfully."));
//        }
//    }
//}