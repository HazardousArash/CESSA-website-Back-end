namespace Cessa.Application.Features.HallOfFame
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public record HallOfFameDto(
        Guid Id, 
        string FullName, 
        string Description, 
        string? GithubUrl, 
        string? LinkedInUrl, 
        string? TelegramId, 
        string? Email, 
        string? PhotoUrl,
        int? EntryYear,
        bool IsAssociationMember,
        string? Term,
        string? RoleTitle);

    public record GetHallOfFameQuery(bool? IsAssociation = null) : IRequest<List<HallOfFameDto>>;

    public class GetHallOfFameQueryHandler(IApplicationDbContext context) : IRequestHandler<GetHallOfFameQuery, List<HallOfFameDto>>
    {
        public async Task<List<HallOfFameDto>> Handle(GetHallOfFameQuery request, CancellationToken cancellationToken)
        {
            var query = context.HallOfFameMembers
                .Include(h => h.PhotoFile)
                .Where(h => !h.IsDeleted)
                .AsQueryable();

            if (request.IsAssociation.HasValue)
            {
                query = query.Where(h => h.IsAssociationMember == request.IsAssociation.Value);
            }

            var members = await query.ToListAsync(cancellationToken);

            var sorted = members.OrderByDescending(h => h.IsAssociationMember ? h.Term : null)
                                .ThenByDescending(h => h.EntryYear)
                                .ThenByDescending(h => h.CreatedAt);

            return sorted.Select(h => new HallOfFameDto(
                h.Id, 
                h.FullName, 
                h.Description, 
                h.GithubUrl, 
                h.LinkedInUrl, 
                h.TelegramId, 
                h.Email, 
                h.PhotoFile != null ? h.PhotoFile.FilePath : null,
                h.EntryYear,
                h.IsAssociationMember,
                h.Term,
                h.RoleTitle
            )).ToList();
        }
    }
}
