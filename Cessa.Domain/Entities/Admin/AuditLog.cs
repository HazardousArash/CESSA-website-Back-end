using Cessa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cessa.Domain.Entities.Admin
{
    public class AuditLog : BaseEntity
    {
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public Guid? AdminId { get; set; }
    }
}
