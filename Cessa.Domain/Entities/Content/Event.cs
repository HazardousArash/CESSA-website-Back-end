using Cessa.Domain.Common;
using Cessa.Domain.Entities.Auth;

namespace Cessa.Domain.Entities.Content
{
    public class Event : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? AdminCardNumber { get; set; }

        public Guid? CoverFileId { get; set; }
        public UploadedFile? CoverFile { get; set; }

        public Guid CreatorId { get; set; }
        public ApplicationUser Creator { get; set; } = null!;

        public ICollection<EventEnrollment> Enrollments { get; set; } = new List<EventEnrollment>();
    }
}