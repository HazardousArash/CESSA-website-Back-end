using Cessa.Domain.Common;
using Cessa.Domain.Entities.Auth;
using System;
using System.Collections.Generic;

namespace Cessa.Domain.Entities.Content
{
    public class PostComment : BaseEntity
    {
        public string Text { get; set; } = string.Empty;
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        // Added for Nested Replies
        public Guid? ParentCommentId { get; set; }
        public PostComment? ParentComment { get; set; }
        public ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
    }
}