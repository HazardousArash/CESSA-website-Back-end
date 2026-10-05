namespace Cessa.Application.Features.Student.Enrollments
{
    using MediatR;
    using Cessa.Domain.Entities.Content;
    using Cessa.Domain.Enums;
    using Cessa.Application.Common.Interfaces;
    using Cessa.Application.Common.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public record EnrollInEventCommand(Guid EventId, Guid UserId, Guid? InvoiceFileId) : IRequest<bool>;

    public class EnrollInEventHandler(IApplicationDbContext context) : IRequestHandler<EnrollInEventCommand, bool>
    {
        public async Task<bool> Handle(EnrollInEventCommand request, CancellationToken cancellationToken)
        {
            var targetEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
                ?? throw new NotFoundException(nameof(Event), request.EventId);

            // 1. SECURITY FIX: Prevent duplicate enrollment spamming
            var alreadyEnrolled = await context.EventEnrollments
                .AnyAsync(e => e.EventId == request.EventId && e.UserId == request.UserId && !e.IsDeleted, cancellationToken);
            if (alreadyEnrolled)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Enrollment", new[] { "شما قبلاً در این رویداد ثبت‌نام کرده‌اید." } }
                });
            }

            // 2. SECURITY FIX: Verify invoice file ownership to prevent linking arbitrary files
            if (request.InvoiceFileId.HasValue)
            {
                var ownsFile = await context.UploadedFiles
                    .AnyAsync(f => f.Id == request.InvoiceFileId.Value && f.UploadedById == request.UserId && !f.IsDeleted, cancellationToken);
                if (!ownsFile)
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "Invoice", new[] { "رسید انتخاب‌شده نامعتبر است یا متعلق به شما نمی‌باشد." } }
                    });
                }
            }

            var enrollment = new EventEnrollment
            {
                EventId = request.EventId,
                UserId = request.UserId,
                InvoiceFileId = request.InvoiceFileId,
                Status = EnrollmentStatus.Pending
            };

            context.EventEnrollments.Add(enrollment);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
