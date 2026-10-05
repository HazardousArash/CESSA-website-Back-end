namespace Cessa.Application.Features.Posts
{
    using MediatR;
    using Cessa.Domain.Entities.Content;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    public record ToggleLikeCommand(Guid PostId, Guid UserId) : IRequest<bool>;
    public class ToggleLikeCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleLikeCommand, bool>
    {
        public async Task<bool> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
        {
            var existingLike = await context.PostLikes.FirstOrDefaultAsync(l => l.PostId == request.PostId && l.UserId == request.UserId, cancellationToken);
            if (existingLike != null) { context.PostLikes.Remove(existingLike); }
            else { context.PostLikes.Add(new PostLike { PostId = request.PostId, UserId = request.UserId }); }
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public record AddCommentCommand(Guid PostId, Guid UserId, string Text, Guid? ParentCommentId) : IRequest<Guid>;
    public class AddCommentCommandHandler(IApplicationDbContext context) : IRequestHandler<AddCommentCommand, Guid>
    {
        public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Trim().Length > 1000) throw new Exception("Invalid comment length.");

            var parentId = request.ParentCommentId;
            if (parentId.HasValue)
            {
                var parent = await context.PostComments.FindAsync(parentId.Value);
                if (parent != null && parent.ParentCommentId.HasValue) {
                    parentId = parent.ParentCommentId;
                }
            }

            var comment = new PostComment { PostId = request.PostId, UserId = request.UserId, Text = request.Text.Trim(), ParentCommentId = parentId };
            context.PostComments.Add(comment);
            await context.SaveChangesAsync(cancellationToken);
            return comment.Id;
        }
    }

    public record DeleteCommentCommand(Guid PostId, Guid CommentId, Guid UserId, string Role) : IRequest<bool>;
    public class DeleteCommentCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteCommentCommand, bool>
    {
        public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await context.PostComments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == request.CommentId && c.PostId == request.PostId, cancellationToken);
            bool canDelete = request.Role == "Admin" || comment.UserId == request.UserId || (request.Role == "ContentCreator" && comment.Post.AuthorId == request.UserId);
            if (canDelete && comment != null) { comment.IsDeleted = true; await context.SaveChangesAsync(cancellationToken); }
            return true;
        }
    }

    public record CommentDto(Guid Id, string Text, Guid UserId, string UserName, string? ProfilePictureUrl, DateTime CreatedAt, Guid? ParentCommentId, List<CommentDto> Replies);
    public record GetPostCommentsQuery(Guid PostId) : IRequest<List<CommentDto>>;
    public class GetPostCommentsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetPostCommentsQuery, List<CommentDto>>
    {
        public async Task<List<CommentDto>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
        {
            var comments = await context.PostComments.Include(c => c.User).ThenInclude(u => u.ProfilePicture).Where(c => c.PostId == request.PostId && !c.IsDeleted).OrderBy(c => c.CreatedAt).Take(200).ToListAsync(cancellationToken);
            var dict = comments.ToDictionary(c => c.Id, c => new CommentDto(c.Id, c.Text, c.UserId, $"{c.User.FirstName} {c.User.LastName}", c.User.ProfilePicture?.FilePath, c.CreatedAt, c.ParentCommentId, new List<CommentDto>()));
            var rootComments = new List<CommentDto>();
            foreach (var c in dict.Values) {
                if (c.ParentCommentId.HasValue && dict.ContainsKey(c.ParentCommentId.Value)) dict[c.ParentCommentId.Value].Replies.Add(c);
                else rootComments.Add(c);
            }
            return rootComments.OrderByDescending(c => c.CreatedAt).ToList();
        }
    }
}
