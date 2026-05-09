using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public string TenantId { get; set; } // Foreign key to ApplicationUser
        public string Subject { get; set; } // e.g., "Leaking Faucet in Kitchen"
        public string Description { get; set; }

        public DateTime DateSubmitted { get; set; } = DateTime.Now;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public string? AdminResponse { get; set; }

        // Navigation property
        public ApplicationUser Tenant { get; set; }
    }
}
