namespace Cessa.Application.Features.Events
{
    using MediatR;
    using Cessa.Domain.Enums;
    using Cessa.Domain.Entities.Content;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public record EnrollmentDto(Guid Id, Guid UserId, string UserName, EnrollmentStatus Status, string? InvoiceUrl, DateTime CreatedAt);

    public record GetEventEnrollmentsQuery(Guid EventId, Guid RequestingUserId, bool IsAdmin) : IRequest<List<EnrollmentDto>>;

    public class GetEventEnrollmentsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetEventEnrollmentsQuery, List<EnrollmentDto>>
    {
        public async Task<List<EnrollmentDto>> Handle(GetEventEnrollmentsQuery request, CancellationToken cancellationToken)
        {
            var ev = await context.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
                ?? throw new NotFoundException(nameof(Event), request.EventId);

            if (!request.IsAdmin && ev.CreatorId != request.RequestingUserId)
                throw new UnauthorizedAccessException("مجوز ندارید");

            return await context.EventEnrollments
                .Include(e => e.User)
                .Include(e => e.InvoiceFile)
                .Where(e => e.EventId == request.EventId)
                .Select(e => new EnrollmentDto(e.Id, e.UserId, $"{e.User.FirstName} {e.User.LastName}", e.Status, e.InvoiceFile != null ? e.InvoiceFile.FilePath : null, e.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }

    // Wrap status to avoid JSON primitive binding errors
    public record UpdateStatusRequest(EnrollmentStatus Status);

    public record UpdateEnrollmentStatusCommand(Guid EventId, Guid EnrollmentId, EnrollmentStatus Status, Guid RequestingUserId, bool IsAdmin) : IRequest<bool>;

    public class UpdateEnrollmentStatusCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateEnrollmentStatusCommand, bool>
    {
        public async Task<bool> Handle(UpdateEnrollmentStatusCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await context.EventEnrollments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId && e.EventId == request.EventId, cancellationToken)
                ?? throw new NotFoundException(nameof(EventEnrollment), request.EnrollmentId);

            if (!request.IsAdmin && enrollment.Event.CreatorId != request.RequestingUserId)
                throw new UnauthorizedAccessException("مجوز ندارید");

            enrollment.Status = request.Status;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}