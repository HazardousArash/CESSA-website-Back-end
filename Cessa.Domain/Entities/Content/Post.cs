namespace Cessa.Domain.Entities.Content
{
    using Cessa.Domain.Common;
    using Cessa.Domain.Entities.Auth;
    using System;
    using System.Collections.Generic;

    public class Post : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Link1 { get; set; }
        public string? Link2 { get; set; }
        public string? Link3 { get; set; }
        public Guid? CoverFileId { get; set; }
        public UploadedFile? CoverFile { get; set; }
        
        public Guid? CategoryId { get; set; }
        public PostCategory? Category { get; set; }

        public Guid AuthorId { get; set; }
        public ApplicationUser Author { get; set; } = null!;

        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
        public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    }
}
