using Cessa.Domain.Common;
using Cessa.Domain.Entities.Auth;

namespace Cessa.Domain.Entities.Content
{
    public class PostLike : BaseEntity
    {
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
