using Cessa.Domain.Common;
using Cessa.Domain.Entities.Auth;

namespace Cessa.Domain.Entities.Content
{
    public class UploadedFile : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public Guid UploadedById { get; set; }
        public ApplicationUser UploadedBy { get; set; } = null!;
    }
}