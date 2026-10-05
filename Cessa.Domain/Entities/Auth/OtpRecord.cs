using Cessa.Domain.Common;

namespace Cessa.Domain.Entities.Auth
{
    public class OtpRecord : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
