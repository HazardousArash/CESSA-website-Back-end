namespace Cessa.Api.Controllers.Areas.Admin
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Api.Helpers;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Domain.Entities.Content;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class CategoriesController(IApplicationDbContext context) : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            context.PostCategories.Add(new PostCategory { Name = name.Trim() });
            await context.SaveChangesAsync(default);
            return Ok(ApiResult.Success("Category created."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var cat = await context.PostCategories.FindAsync(id);
            if (cat != null) { cat.IsDeleted = true; await context.SaveChangesAsync(default); }
            return Ok(ApiResult.Success("Category deleted."));
        }
    }
}
