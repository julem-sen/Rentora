using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Rentora.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? TenantAccountNumber { get; set; }
        public string? UnitNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } 
        public bool IsActive { get; set; } = true;

        public decimal? MonthlyRentAmount { get; set; }
        public int? AdvanceMonthsRemaining { get; set; } 
        public decimal? SecurityDepositBalance { get; set; }
        public DateTime? LeaseStartDate { get; set; } 

        // Navigation Properties
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
