namespace Cessa.Domain.Entities.Content
{
    using Cessa.Domain.Common;
    using Cessa.Domain.Entities.Auth;
    using System;
    public class UserCategoryPreference : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public PostCategory Category { get; set; } = null!;
        public int ClickCount { get; set; } = 0;
    }
}
