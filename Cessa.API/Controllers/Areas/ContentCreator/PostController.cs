namespace Cessa.Api.Controllers.Areas.ContentCreator
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Cessa.Application.Features.Posts;
    using Cessa.Api.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public record AddCommentRequest(string Text, Guid? ParentCommentId);

    [Authorize(Roles = "ContentCreator")]
    [Route("api/creator/[controller]")]
    [ApiExplorerSettings(GroupName = "contentcreator")]
    public class PostController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetRecentPosts()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await Mediator.Send(new GetRecentPostsQuery(userId));
            return Ok(ApiResult.Success(result, "Recent posts retrieved successfully."));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await Mediator.Send(new GetPostByIdQuery(id, userId));
            return Ok(ApiResult.Success(result, "Post retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command)
        {
            var authorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var newCommand = command with { AuthorId = authorId };
            var result = await Mediator.Send(newCommand);
            return StatusCode(StatusCodes.Status201Created, ApiResult.Created(result, "Post created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostCommand command)
        {
            if (id != command.Id) return BadRequest(ApiResult.BadRequest("ID mismatch in route and body."));

            var authorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var newCommand = command with { AuthorId = authorId };
            await Mediator.Send(newCommand);
            return Ok(ApiResult.Success("Post updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var authorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await Mediator.Send(new DeletePostCommand(id, authorId));
            return Ok(ApiResult.Success("Post deleted successfully."));
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await Mediator.Send(new ToggleLikeCommand(id, userId));
            return Ok(ApiResult.Success("Like toggled successfully."));
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await Mediator.Send(new AddCommentCommand(id, userId, request.Text, request.ParentCommentId));
            return Ok(ApiResult.Success("Comment added successfully."));
        }

        [HttpDelete("{id}/comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await Mediator.Send(new DeleteCommentCommand(id, commentId, userId, "ContentCreator"));
            return Ok(ApiResult.Success("Comment deleted successfully."));
        }

        [AllowAnonymous]
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(Guid id)
        {
            var result = await Mediator.Send(new GetPostCommentsQuery(id));
            return Ok(ApiResult.Success(result, "Comments retrieved successfully."));
        }
    }
}