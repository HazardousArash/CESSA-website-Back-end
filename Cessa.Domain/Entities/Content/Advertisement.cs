namespace Cessa.Domain.Entities.Content
{
    using Cessa.Domain.Common;
    using System;

    public class Advertisement : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid? CoverFileId { get; set; }
        public UploadedFile? CoverFile { get; set; }
    }
}