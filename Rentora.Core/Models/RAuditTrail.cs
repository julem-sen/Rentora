using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models
{
    public class RAuditTrail
    {
        public int Id { get; set; }
        public string UserId { get; set; } // The Admin or Tenant who made the change
        public string Action { get; set; } // e.g., "Created", "Updated", "Deleted"
        public string EntityName { get; set; } // e.g., "Invoice", "Payment"
        public string EntityId { get; set; } // The ID of the modified record
        public string? OldValues { get; set; } // Stored as a JSON string
        public string? NewValues { get; set; } // Stored as a JSON string
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
