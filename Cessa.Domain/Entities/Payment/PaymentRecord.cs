using Cessa.Domain.Common;
using Cessa.Domain.Entities.Auth;

namespace Cessa.Domain.Entities.Payment
{
    public class PaymentRecord : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
