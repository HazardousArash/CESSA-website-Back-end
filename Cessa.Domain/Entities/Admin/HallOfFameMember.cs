namespace Cessa.Domain.Entities.Admin
{
    using Cessa.Domain.Common;
    using Cessa.Domain.Entities.Content;

    public class HallOfFameMember : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? GithubUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? TelegramId { get; set; }
        public string? Email { get; set; }

        public int? EntryYear { get; set; }
        public bool IsAssociationMember { get; set; } = false;
        public string? Term { get; set; }
        public string? RoleTitle { get; set; }

        public Guid? PhotoFileId { get; set; }
        public UploadedFile? PhotoFile { get; set; }
    }
}
