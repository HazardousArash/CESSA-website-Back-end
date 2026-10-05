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

    public record CreatePostCommand(string Title, string Content, Guid? CoverFileId, Guid AuthorId, string? Link1, string? Link2, string? Link3, Guid? CategoryId) : IRequest<Guid>;
    public class CreatePostCommandHandler(IApplicationDbContext context) : IRequestHandler<CreatePostCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FindAsync(new object[] { request.AuthorId }, cancellationToken);
            if (user != null && !user.IsVerifiedCreator) throw new UnauthorizedAccessException("حساب شما تایید نشده است.");

            var post = new Post { Title = request.Title, Content = request.Content, CoverFileId = request.CoverFileId, AuthorId = request.AuthorId, Link1 = request.Link1, Link2 = request.Link2, Link3 = request.Link3, CategoryId = request.CategoryId };
            context.Posts.Add(post);
            await context.SaveChangesAsync(cancellationToken);
            return post.Id;
        }
    }

    public record UpdatePostCommand(Guid Id, string Title, string Content, Guid? CoverFileId, Guid AuthorId, string? Link1, string? Link2, string? Link3, Guid? CategoryId) : IRequest<bool>;
    public class UpdatePostCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdatePostCommand, bool>
    {
        public async Task<bool> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
            var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == request.Id && p.AuthorId == request.AuthorId, cancellationToken) ?? throw new NotFoundException(nameof(Post), request.Id);
            post.Title = request.Title; post.Content = request.Content; post.CoverFileId = request.CoverFileId; post.Link1 = request.Link1; post.Link2 = request.Link2; post.Link3 = request.Link3; post.CategoryId = request.CategoryId;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public record DeletePostCommand(Guid Id, Guid AuthorId) : IRequest<bool>;
    public class DeletePostCommandHandler(IApplicationDbContext context) : IRequestHandler<DeletePostCommand, bool>
    {
        public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == request.Id && p.AuthorId == request.AuthorId, cancellationToken) ?? throw new NotFoundException(nameof(Post), request.Id);
            post.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
