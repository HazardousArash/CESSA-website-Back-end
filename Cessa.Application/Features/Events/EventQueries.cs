namespace Cessa.Application.Features.Events
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Cessa.Domain.Entities.Content;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public record EventDto(
        Guid Id, 
        string Title, 
        string Description, 
        DateTime EventDate, 
        string? AdminCardNumber, 
        string? CoverFilePath, 
        Guid CreatorId, 
        string CreatorName,
        string? CreatorProfilePictureUrl,
        bool IsEnrolledByCurrentUser, 
        int? CurrentUserEnrollmentStatus,
        bool HasCardNumber = false);

    public record GetEventsQuery(Guid? UserId) : IRequest<List<EventDto>>;

    public class GetEventsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetEventsQuery, List<EventDto>>
    {
        public async Task<List<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            return await context.Events
                .Include(e => e.CoverFile)
                .Include(e => e.Enrollments)
                .Include(e => e.Creator).ThenInclude(c => c.ProfilePicture)
                .OrderByDescending(e => e.EventDate)
                .Select(e => new EventDto(
                    e.Id,
                    e.Title,
                    e.Description,
                    e.EventDate,
                    request.UserId.HasValue ? e.AdminCardNumber : null,
                    e.CoverFile != null ? e.CoverFile.FilePath : null,
                    e.CreatorId,
                    $"{e.Creator.FirstName} {e.Creator.LastName}",
                    e.Creator.ProfilePicture != null ? e.Creator.ProfilePicture.FilePath : null,
                    request.UserId.HasValue && e.Enrollments.Any(en => en.UserId == request.UserId.Value),
                    request.UserId.HasValue ? (int?)e.Enrollments.FirstOrDefault(en => en.UserId == request.UserId.Value)!.Status : null,
                    !string.IsNullOrEmpty(e.AdminCardNumber)))
                .ToListAsync(cancellationToken);
        }
    }

    public record GetEventByIdQuery(Guid Id, Guid? UserId) : IRequest<EventDto>;

    public class GetEventByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetEventByIdQuery, EventDto>
    {
        public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var ev = await context.Events
                .Include(e => e.CoverFile)
                .Include(e => e.Enrollments)
                .Include(e => e.Creator).ThenInclude(c => c.ProfilePicture)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Event), request.Id);

            var myEnrollment = request.UserId.HasValue ? ev.Enrollments.FirstOrDefault(en => en.UserId == request.UserId.Value) : null;
            bool isEnrolled = myEnrollment != null;

            bool hasCard = !string.IsNullOrWhiteSpace(ev.AdminCardNumber);
            string? cardNumber = request.UserId.HasValue ? ev.AdminCardNumber : null;

            return new EventDto(
                ev.Id,
                ev.Title,
                ev.Description,
                ev.EventDate,
                cardNumber,
                ev.CoverFile != null ? ev.CoverFile.FilePath : null,
                ev.CreatorId,
                $"{ev.Creator.FirstName} {ev.Creator.LastName}",
                ev.Creator.ProfilePicture != null ? ev.Creator.ProfilePicture.FilePath : null,
                isEnrolled,
                myEnrollment != null ? (int)myEnrollment.Status : null,
                hasCard);
        }
    }
}
