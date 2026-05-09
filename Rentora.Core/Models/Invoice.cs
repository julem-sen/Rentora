using Microsoft.EntityFrameworkCore;
using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string TenantId { get; set; }

        // Dynamically calculates the total based on the line items
        public decimal TotalAmount => Items?.Sum(i => i.Amount) ?? 0;

        public DateTime DateIssued { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }

        // Navigation Properties
        public ApplicationUser Tenant { get; set; }
        public ICollection<InvoiceItem> Items { get; set; } // The new line items
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Document> Documents { get; set; }
    }
}
