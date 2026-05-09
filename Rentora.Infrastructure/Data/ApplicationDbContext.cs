using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rentora.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rentora.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<RErrorLog> RErrorLogs { get; set; }
        public DbSet<RAuditTrail> RAuditTrails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure relationships and constraints
            modelBuilder.Entity<Payment>()
            .HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Document>()
            .HasOne(d => d.Invoice)
            .WithMany(i => i.Documents)
            .HasForeignKey(d => d.InvoiceId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.MonthlyRentAmount)
            .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.Amount)
            .HasPrecision(18, 2);
            
            modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.SecurityDepositBalance)
            .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
            .Property(i => i.AmountPaid)
            .HasPrecision(18, 2);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Get the current user ID from the JWT Token (or set to "System" for background workers)
            string currentUserId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

            // 2. Find all entities that are Added, Modified, or Deleted
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not RAuditTrail && e.Entity is not RErrorLog) // NEVER audit the audit table!
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            var auditEntries = new List<RAuditTrail>();

            foreach (var entry in entries)
            {
                var audit = new RAuditTrail
                {
                    UserId = currentUserId,
                    Action = entry.State.ToString(),
                    EntityName = entry.Entity.GetType().Name,
                    Timestamp = DateTime.UtcNow
                };

                // Try to grab the Primary Key (Id)
                var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                audit.EntityId = primaryKey?.CurrentValue?.ToString() ?? "Unknown";

                // Map the Old and New values to JSON depending on what happened
                switch (entry.State)
                {
                    case EntityState.Added:
                        audit.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                        break;

                    case EntityState.Deleted:
                        audit.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                        break;

                    case EntityState.Modified:
                        audit.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                        audit.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                        break;
                }

                auditEntries.Add(audit);
            }

            // 3. Add the audit trails to the database BEFORE saving
            if (auditEntries.Any())
            {
                RAuditTrails.AddRange(auditEntries);
            }

            // 4. Finally, execute the actual save to SQL Server
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
