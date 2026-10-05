namespace Cessa.Application.Features.Student.Payments
{
    using MediatR;
    using Cessa.Application.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public record PaymentDto(
        Guid Id,
        string Reason,
        decimal Amount,
        DateTime PaymentDate,
        string TrackingNumber,
        string? InvoiceUrl,
        string UserName,
        int Status
    );

    public record GetMyPaymentsQuery(Guid UserId, string Role) : IRequest<List<PaymentDto>>;

    public class GetMyPaymentsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetMyPaymentsQuery, List<PaymentDto>>
    {
        public async Task<List<PaymentDto>> Handle(GetMyPaymentsQuery request, CancellationToken cancellationToken)
        {
            // Return all enrollments with an uploaded receipt
            var query = context.EventEnrollments
                .Include(e => e.Event)
                .Include(e => e.User)
                .Include(e => e.InvoiceFile)
                .Where(e => e.InvoiceFileId != null && !e.IsDeleted);

            if (request.Role == "Student")
            {
                query = query.Where(e => e.UserId == request.UserId);
            }
            else if (request.Role == "ContentCreator")
            {
                query = query.Where(e => e.Event.CreatorId == request.UserId);
            }
            // Admin receives all records

            return await query.OrderByDescending(e => e.CreatedAt)
                .Select(e => new PaymentDto(
                    e.Id,
                    $"ثبت‌نام در: {e.Event.Title}",
                    0,
                    e.CreatedAt,
                    e.Id.ToString().Substring(0, 8).ToUpper(),
                    e.InvoiceFile != null ? e.InvoiceFile.FilePath : null,
                    $"{e.User.FirstName} {e.User.LastName}",
                    (int)e.Status))
                .ToListAsync(cancellationToken);
        }
    }
}