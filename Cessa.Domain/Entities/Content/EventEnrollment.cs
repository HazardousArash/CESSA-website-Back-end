using Cessa.Domain.Common;
using Cessa.Domain.Entities.Auth;
using Cessa.Domain.Enums;

namespace Cessa.Domain.Entities.Content
{
    public class EventEnrollment : BaseEntity
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
        public Guid? InvoiceFileId { get; set; }
        public UploadedFile? InvoiceFile { get; set; }
    }
}