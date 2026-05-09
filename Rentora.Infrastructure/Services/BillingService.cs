using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillingService> _logger;

        public BillingService(ApplicationDbContext context, ILogger<BillingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task GenerateMonthlyInvoicesAsync()
        {
            // 1. TIMEZONE FIX: Force Philippine Standard Time
            // Note: On Windows servers, the ID is "Taipei Standard Time" or "Singapore Standard Time". 
            // On Linux (Docker/Cloud), it is usually "Asia/Manila".
            var phTime = GetPhilippineTime();
            _logger.LogInformation("Billing Job started for Due Month: {MonthYear}", phTime.ToString("MMMM yyyy"));

            var tenantRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Tenant");
            if (tenantRole == null)
            {
                _logger.LogError("Billing Job failed: 'Tenant' role not found in database.");
                return;
            }

            var activeTenants = await _context.Users
                .Where(u => u.IsActive && _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == tenantRole.Id))
                .Include(u => u.Invoices.Where(i => i.Status == InvoiceStatus.Pending))
                .ToListAsync();

            var alreadyBilledTenantIds = await _context.Invoices
                .Where(i => i.DueDate.Month == phTime.Month &&
                            i.DueDate.Year == phTime.Year &&
                            i.Status != InvoiceStatus.Cancelled)
                .Select(i => i.TenantId)
                .ToListAsync();

            var tenantsToNotify = new List<ApplicationUser>();

            foreach (var tenant in activeTenants)
            {
                if (alreadyBilledTenantIds.Contains(tenant.Id))
                {
                    _logger.LogInformation("Skipping Tenant {Id}: Already has an invoice due in {Month}", tenant.Id, phTime.Month);
                    continue;
                }

                try
                {
                    var invoice = CreateInvoiceRecord(tenant, phTime);
                    _context.Invoices.Add(invoice);
                    tenantsToNotify.Add(tenant);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate invoice for tenant {Id}", tenant.Id);
                }
            }

            if (tenantsToNotify.Any())
            {
                await _context.SaveChangesAsync();

                foreach (var tenant in tenantsToNotify)
                {
                    EnqueueInvoiceEmail(tenant, phTime);
                }
            }
        }

        private Invoice CreateInvoiceRecord(ApplicationUser tenant, DateTime phTime)
        {
            int daysInMonth = DateTime.DaysInMonth(phTime.Year, phTime.Month);
            int dueDay = Math.Min(daysInMonth, tenant.LeaseStartDate?.Day ?? 1);
            DateTime dueDate = new DateTime(phTime.Year, phTime.Month, dueDay);

            var invoice = new Invoice
            {
                TenantId = tenant.Id,
                DateIssued = phTime, // Current creation time
                DueDate = dueDate,   // The actual billing target
                Status = InvoiceStatus.Pending,
                Items = new List<InvoiceItem>()
            };

            // Rent Logic
            if (tenant.AdvanceMonthsRemaining > 0)
            {
                invoice.Items.Add(new InvoiceItem { Description = "Rent (Advance Applied)", Amount = 0 });
                tenant.AdvanceMonthsRemaining--;
            }
            else
            {
                invoice.Items.Add(new InvoiceItem { Description = "Monthly Rent", Amount = tenant.MonthlyRentAmount ?? 0 });
            }

            // Rollover Arrears
            var unpaid = tenant.Invoices.Where(i => i.Status == InvoiceStatus.Pending).ToList();
            if (unpaid.Any())
            {
                invoice.Items.Add(new InvoiceItem { Description = "Previous Arrears", Amount = unpaid.Sum(x => x.TotalAmount) });
                foreach (var old in unpaid) old.Status = InvoiceStatus.RolledOver;
            }

            return invoice;
        }

        private void EnqueueInvoiceEmail(ApplicationUser tenant, DateTime phTime)
        {
            string subject = $"Invoice Ready - {phTime:MMMM yyyy}";
            string body = $"Hi {tenant.FullName}, your invoice for {phTime:MMMM yyyy} is now available in the portal.";

            BackgroundJob.Enqueue<IEmailService>(svc => svc.SendEmailAsync(tenant.Email, subject, body));
        }

        private DateTime GetPhilippineTime()
        {
            var zone = OperatingSystem.IsWindows() ? "Taipei Standard Time" : "Asia/Manila";
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(zone));
        }
    }
}
