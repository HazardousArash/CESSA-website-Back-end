namespace Cessa.Domain.Entities.Auth
{
    using Cessa.Domain.Common;
    using Cessa.Domain.Entities.Content;
    using Microsoft.AspNetCore.Identity;
    using System;

    public class ApplicationUser : IdentityUser<Guid>, IBaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
        public int? EntryYear { get; set; }

        public bool IsVerifiedCreator { get; set; } = false;

        // Profile Picture
        public Guid? ProfilePictureId { get; set; }
        public UploadedFile? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}