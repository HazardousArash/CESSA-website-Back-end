namespace Cessa.Api.Controllers.Areas.Admin
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Posts;
    using Cessa.Api.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using AdminCreatePostCommand = Cessa.Application.Features.Admin.Posts.CreatePostCommand;
    using AdminUpdatePostCommand = Cessa.Application.Features.Admin.Posts.UpdatePostCommand;
    using AdminDeletePostCommand = Cessa.Application.Features.Admin.Posts.DeletePostCommand;

    public record AddCommentRequest(string Text, Guid? ParentCommentId);

    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class PostController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetRecentPosts()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid? userId = Guid.TryParse(userIdStr, out var id) ? id : null;
            var result = await Mediator.Send(new GetRecentPostsQuery(userId));
            return Ok(ApiResult.Success(result, "Recent posts retrieved successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid? userId = Guid.TryParse(userIdStr, out var uId) ? uId : null;
            var result = await Mediator.Send(new GetPostByIdQuery(id, userId));
            return Ok(ApiResult.Success(result, "Post retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] AdminCreatePostCommand command)
        {
            var authorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var authorId = Guid.TryParse(authorIdStr, out var id) ? id : command.AuthorId;
            var newCommand = command with { AuthorId = authorId };
            var result = await Mediator.Send(newCommand);
            return StatusCode(StatusCodes.Status201Created, ApiResult.Created(result, "Post created successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] AdminUpdatePostCommand command)
        {
            if (id != command.Id) return BadRequest(ApiResult.BadRequest("ID mismatch in route and body."));
            await Mediator.Send(command);
            return Ok(ApiResult.Success("Post updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            await Mediator.Send(new AdminDeletePostCommand(id));
            return Ok(ApiResult.Success("Post deleted successfully."));
        }

        [HttpPost("{id:guid}/like")]
        public async Task<IActionResult> ToggleLike(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            await Mediator.Send(new ToggleLikeCommand(id, userId));
            return Ok(ApiResult.Success("Like toggled successfully."));
        }

        [HttpPost("{id:guid}/comment")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            await Mediator.Send(new AddCommentCommand(id, userId, request.Text, request.ParentCommentId));
            return Ok(ApiResult.Success("Comment added successfully."));
        }

        [HttpDelete("{id:guid}/comment/{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            await Mediator.Send(new DeleteCommentCommand(id, commentId, userId, "Admin"));
            return Ok(ApiResult.Success("Comment deleted successfully."));
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}/comments")]
        public async Task<IActionResult> GetComments(Guid id)
        {
            var result = await Mediator.Send(new GetPostCommentsQuery(id));
            return Ok(ApiResult.Success(result, "Comments retrieved successfully."));
        }
    }
}
