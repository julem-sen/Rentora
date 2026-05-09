using Rentora.Core.DTOs;
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
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateMoveInInvoiceAsync(string tenantId, decimal monthlyRentAmount, int monthsAdvance, int monthsDeposit)
        {
            var moveInInvoice = new Invoice
            {
                TenantId = tenantId,
                DateIssued = DateTime.Now,
                DueDate = DateTime.Now.AddMonths(monthsAdvance), // Add Months base on the Advanced Payment
                Status = InvoiceStatus.Pending,
                Items = new List<InvoiceItem>()
            };

            // Add Advance Line Item
            if (monthsAdvance > 0)
            {
                moveInInvoice.Items.Add(new InvoiceItem
                {
                    Description = $"{monthsAdvance} Month(s) Advance Rent",
                    Amount = monthlyRentAmount * monthsAdvance
                });
            }

            // Add Deposit Line Item
            if (monthsDeposit > 0)
            {
                moveInInvoice.Items.Add(new InvoiceItem
                {
                    Description = $"{monthsDeposit} Month(s) Security Deposit",
                    Amount = monthlyRentAmount * monthsDeposit
                });
            }

            _context.Invoices.Add(moveInInvoice);
            await _context.SaveChangesAsync();
        }

        public async Task CreateManualInvoiceAsync(CreateManualInvoiceRequest request)
        {
            var manualInvoice = new Invoice
            {
                TenantId = request.TenantId,
                DateIssued = DateTime.Now,
                DueDate = request.DueDate,
                Status = InvoiceStatus.Pending,
                Items = new List<InvoiceItem>()
            };

            // Loop through the DTO's list directly
            foreach (var item in request.Items)
            {
                manualInvoice.Items.Add(new InvoiceItem
                {
                    Description = item.Description,
                    Amount = item.Amount
                });
            }

            _context.Invoices.Add(manualInvoice);
            await _context.SaveChangesAsync();
        }
    }
}
