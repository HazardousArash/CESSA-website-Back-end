namespace Cessa.Application.Features.Users
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public record UserDto(Guid Id, string FirstName, string LastName, string Email,
        string? StudentNumber, int? EntryYear, bool IsVerifiedCreator, List<string> Roles,
        string? ProfilePictureUrl);

    public record GetUsersQuery(string? SearchTerm, int? EntryYear, bool IsAdmin = false) : IRequest<List<UserDto>>;

    public class GetUsersQueryHandler(IApplicationDbContext context) : IRequestHandler<GetUsersQuery, List<UserDto>>
    {
        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = context.Users
                .Include(u => u.ProfilePicture)
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(u => u.FirstName.ToLower().Contains(term) || u.LastName.ToLower().Contains(term) || (u.Email != null && u.Email.ToLower().Contains(term)));
            }

            if (request.EntryYear.HasValue)
            {
                query = query.Where(u => u.EntryYear == request.EntryYear.Value);
            }

            var userRoles = await context.UserRoles.ToListAsync(cancellationToken);
            var roles = await context.Roles.ToListAsync(cancellationToken);
            var dbUsers = await query.ToListAsync(cancellationToken);

            return dbUsers.Select(u => new UserDto(
                u.Id, 
                u.FirstName, 
                u.LastName, 
                // SECURITY FIX: Hide personal email and university student numbers from other students
                request.IsAdmin ? (u.Email ?? "") : "", 
                request.IsAdmin ? u.StudentNumber : null, 
                u.EntryYear, 
                u.IsVerifiedCreator,
                userRoles.Where(ur => ur.UserId == u.Id).Join(roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!).ToList(),
                u.ProfilePicture?.FilePath 
            )).ToList();
        }
    }
}
