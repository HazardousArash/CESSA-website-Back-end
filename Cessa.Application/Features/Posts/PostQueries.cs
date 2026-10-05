namespace Cessa.Application.Features.Posts
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cessa.Application.Common.Exceptions;
    using Cessa.Domain.Entities.Content;

    public record PostDto(Guid Id, string Title, string Content, string? CoverFilePath, Guid AuthorId, string AuthorName, string? AuthorProfilePictureUrl, DateTime CreatedAt, int LikesCount, int CommentsCount, bool IsLikedByCurrentUser, string? Link1, string? Link2, string? Link3, Guid? CategoryId, string? CategoryName, List<PostDto>? RelatedPosts = null);

    public record GetRecentPostsQuery(Guid? UserId) : IRequest<List<PostDto>>;
    public class GetRecentPostsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetRecentPostsQuery, List<PostDto>>
    {
        public async Task<List<PostDto>> Handle(GetRecentPostsQuery request, CancellationToken cancellationToken)
        {
            var posts = await context.Posts.Include(p => p.Author).ThenInclude(a => a.ProfilePicture).Include(p => p.CoverFile).Include(p => p.Category).Include(p => p.Likes).Include(p => p.Comments).Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedAt).Take(20).ToListAsync(cancellationToken);
            
            return posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.CoverFile?.FilePath, p.AuthorId, 
                p.Author != null ? $"{p.Author.FirstName} {p.Author.LastName}" : "Unknown", 
                p.Author != null && p.Author.ProfilePicture != null ? p.Author.ProfilePicture.FilePath : null, 
                p.CreatedAt, p.Likes?.Count ?? 0, p.Comments?.Count ?? 0, request.UserId != null && p.Likes != null && p.Likes.Any(l => l.UserId == request.UserId), p.Link1, p.Link2, p.Link3, p.CategoryId, p.Category?.Name)).ToList();
        }
    }

    public record GetPostByIdQuery(Guid Id, Guid? UserId) : IRequest<PostDto>;
    public class GetPostByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetPostByIdQuery, PostDto>
    {
        public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
        {
            var p = await context.Posts.Include(x => x.Author).ThenInclude(a => a.ProfilePicture).Include(x => x.CoverFile).Include(x => x.Category).Include(x => x.Likes).Include(x => x.Comments).FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken) ?? throw new NotFoundException(nameof(Post), request.Id);
            
            if (request.UserId.HasValue && p.CategoryId.HasValue)
            {
                var pref = await context.UserCategoryPreferences.FirstOrDefaultAsync(x => x.UserId == request.UserId.Value && x.CategoryId == p.CategoryId.Value, cancellationToken);
                if (pref == null) { context.UserCategoryPreferences.Add(new UserCategoryPreference { UserId = request.UserId.Value, CategoryId = p.CategoryId.Value, ClickCount = 1 }); }
                else { pref.ClickCount++; }
                await context.SaveChangesAsync(cancellationToken);
            }

            var related = new List<PostDto>();
            if (p.CategoryId.HasValue)
            {
                var relPosts = await context.Posts.Include(x => x.Author).ThenInclude(a => a.ProfilePicture).Include(x => x.CoverFile).Include(x => x.Category).Where(x => !x.IsDeleted && x.CategoryId == p.CategoryId.Value && x.Id != request.Id).OrderByDescending(x => x.CreatedAt).Take(3).ToListAsync(cancellationToken);
                related = relPosts.Select(r => new PostDto(r.Id, r.Title, r.Content, r.CoverFile?.FilePath, r.AuthorId, r.Author != null ? $"{r.Author.FirstName} {r.Author.LastName}" : "Unknown", r.Author != null && r.Author.ProfilePicture != null ? r.Author.ProfilePicture.FilePath : null, r.CreatedAt, 0, 0, false, null, null, null, r.CategoryId, r.Category?.Name)).ToList();
            }

            return new PostDto(p.Id, p.Title, p.Content, p.CoverFile?.FilePath, p.AuthorId, 
                p.Author != null ? $"{p.Author.FirstName} {p.Author.LastName}" : "Unknown", 
                p.Author != null && p.Author.ProfilePicture != null ? p.Author.ProfilePicture.FilePath : null, 
                p.CreatedAt, p.Likes?.Count ?? 0, p.Comments?.Count ?? 0, request.UserId != null && p.Likes != null && p.Likes.Any(l => l.UserId == request.UserId), p.Link1, p.Link2, p.Link3, p.CategoryId, p.Category?.Name, related);
        }
    }
}
