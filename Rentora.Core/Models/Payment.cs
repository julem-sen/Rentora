using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; } // Foreign key to Invoice
        public string TenantId { get; set; }
        public ApplicationUser Tenant { get; set; }
        public PaymentMethod Method { get; set; } // Enum to represent the payment method (e.g., BankTransfer, Cash, QRCode)
        public string TransactionId { get; set; } = Guid.NewGuid().ToString(); // Unique identifier for the payment transaction
        public string? ProofOfPaymentUrl { get; set; } 
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? AdminNotes { get; set; } 


        // Navigation property
        public Invoice Invoice { get; set; }
    }
}
