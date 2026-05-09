using Microsoft.EntityFrameworkCore;
using Rentora.Core.Interfaces;
using Rentora.Core.Models;
using Rentora.Core.Models.Enums;
using Rentora.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> SubmitPaymentAsync(int invoiceId, string tenantId, decimal amountPaid, PaymentMethod method, string? proofOfPaymentUrl)
        {
            // 1. Verify the invoice actually exists and belongs to this tenant
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                return (false, "Invalid invoice or unauthorized access.");
            }

            // 2. Create the pending payment record
            var payment = new Payment
            {
                InvoiceId = invoiceId,
                TenantId = tenantId,
                AmountPaid = amountPaid,
                Method = method,
                ProofOfPaymentUrl = proofOfPaymentUrl,
                Status = PaymentStatus.Pending,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> VerifyPaymentAsync(int paymentId, string? adminNotes)
        {
            // 1. Find the payment and include the connected invoice
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return (false, "Payment not found.");
            }

            if (payment.Status == PaymentStatus.Verified)
            {
                return (false, "This payment has already been verified.");
            }

            // 2. Update the Payment status
            payment.Status = PaymentStatus.Verified;
            payment.AdminNotes = adminNotes;

            // 3. Update the connected Invoice status to Paid
            payment.Invoice.Status = InvoiceStatus.Paid;

            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }
    }
}
