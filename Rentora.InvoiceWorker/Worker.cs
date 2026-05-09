using Microsoft.EntityFrameworkCore;
using Rentora.Core.Models;
using Rentora.Core.Models.Enums;
using Rentora.Infrastructure.Data;

namespace Rentora.InvoiceWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (DateTime.UtcNow.Day == 1)
                {
                    await GenerateMonthlyInvoicesAsync();
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task GenerateMonthlyInvoicesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var activeTenants = await context.Users.Where(u => u.IsActive).ToListAsync();

            foreach (var tenant in activeTenants)
            {
                // Dynamic Due Date Logic
                int currentYear = DateTime.Now.Year;
                int currentMonth = DateTime.Now.Month;
                int dueDay = Math.Min(DateTime.DaysInMonth(currentYear, currentMonth), tenant.LeaseStartDate!.Value.Day);
                DateTime dynamicDueDate = new DateTime(currentYear, currentMonth, dueDay);

                var monthlyInvoice = new Invoice
                {
                    TenantId = tenant.Id,
                    DateIssued = DateTime.Now,
                    DueDate = dynamicDueDate,
                    Status = InvoiceStatus.Pending,
                    Items = new List<InvoiceItem>()
                };

                // Advance Rent Consumption
                if (tenant.AdvanceMonthsRemaining > 0)
                {
                    monthlyInvoice.Items.Add(new InvoiceItem
                    {
                        Description = $"Monthly Rent (Covered by Advance. {tenant.AdvanceMonthsRemaining - 1} left)",
                        Amount = 0
                    });
                    tenant.AdvanceMonthsRemaining -= 1;
                }
                else
                {
                    monthlyInvoice.Items.Add(new InvoiceItem { Description = "Monthly Rent", Amount = tenant.MonthlyRentAmount!.Value });
                }

                // Rollover Debt
                var unpaidInvoices = await context.Invoices
                    .Include(i => i.Items)
                    .Where(i => i.TenantId == tenant.Id && i.Status == InvoiceStatus.Pending)
                    .ToListAsync();

                if (unpaidInvoices.Any())
                {
                    decimal totalArrears = unpaidInvoices.Sum(i => i.TotalAmount);
                    monthlyInvoice.Items.Add(new InvoiceItem { Description = "Previous Unpaid Balance (Arrears)", Amount = totalArrears });

                    foreach (var oldInvoice in unpaidInvoices) oldInvoice.Status = InvoiceStatus.RolledOver;
                }

                context.Invoices.Add(monthlyInvoice);
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Successfully generated {count} monthly invoices.", activeTenants.Count);
        }
    }
}
