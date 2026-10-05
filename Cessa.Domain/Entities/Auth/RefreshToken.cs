namespace Cessa.Domain.Entities.Auth
{
    using Cessa.Domain.Common;
    using System;

    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
