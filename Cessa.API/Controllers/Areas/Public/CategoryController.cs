namespace Cessa.Api.Controllers.Areas.Public
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Api.Helpers;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    [AllowAnonymous]
    [Route("api/public/[controller]")]
    [ApiExplorerSettings(GroupName = "public")]
    public class CategoriesController(IApplicationDbContext context) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cats = await context.PostCategories.Where(c => !c.IsDeleted).Select(c => new { c.Id, c.Name }).ToListAsync();
            return Ok(ApiResult.Success(cats, "Categories retrieved."));
        }
    }
}
