using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;
using Rentora.Core.Models;
using Rentora.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly IInvoiceService _invoiceSvc;
        public TenantService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userMgr,
            IInvoiceService invoiceSvc)
        {
            _context = context;
            _userMgr = userMgr;
            _invoiceSvc = invoiceSvc;
        }

        public async Task<IEnumerable<object>> GetAllTenants()
        {
            // 1. Find the Tenant Role
            var tenantRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Tenant");

            if (tenantRole == null) return Enumerable.Empty<object>();

            // 2. Filter by Role AND ensure the account is Active
            var tenants = await _context.Users
                .Where(u => u.IsActive && _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == tenantRole.Id))
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    u.UnitNumber,
                    u.TenantAccountNumber,
                    u.Email // Adding email so Admin can contact them
                })
                .ToListAsync();

            return tenants;
        }

        public async Task<(bool IsSuccess, string AccountNumber, IEnumerable<string> Errors)> CreateTenantAsync(CreateTenantRequest request)
        {
            string generatedAccountNumber = await GenerateTenantAccountNumberAsync();

            var newTenant = new ApplicationUser
            {
                UserName = generatedAccountNumber,
                Email = request.Email,
                FullName = request.FullName,
                UnitNumber = request.UnitNumber,
                PhoneNumber = request.PhoneNumber,
                TenantAccountNumber = generatedAccountNumber,
                MonthlyRentAmount = request.MonthlyRentAmount,
                AdvanceMonthsRemaining = request.MonthsAdvance,
                SecurityDepositBalance = request.MonthsDeposit * request.MonthlyRentAmount,
                LeaseStartDate = request.LeaseStartDate,
                IsActive = true
            };

            var result = await _userMgr.CreateAsync(newTenant, request.Password);

            if (result.Succeeded)
            {
                await _userMgr.AddToRoleAsync(newTenant, "Tenant");

                // Orchestrate the Move-In Invoice
                await _invoiceSvc.CreateMoveInInvoiceAsync(
                    newTenant.Id,
                    request.MonthlyRentAmount,
                    request.MonthsAdvance,
                    request.MonthsDeposit);

                return (true, generatedAccountNumber, Array.Empty<string>());
            }

            // Extract the errors to send back
            var errors = result.Errors.Select(e => e.Description);
            return (false, string.Empty, errors);
        }

        private async Task<string> GenerateTenantAccountNumberAsync()
        {
            string yearMonth = DateTime.Now.ToString("yyyyMM");

            var lastTenant = await _context.Users
                .Where(u => u.TenantAccountNumber != null && u.TenantAccountNumber.StartsWith(yearMonth))
                .OrderByDescending(u => u.TenantAccountNumber)
                .FirstOrDefaultAsync();

            if (lastTenant == null || string.IsNullOrEmpty(lastTenant.TenantAccountNumber))
            {
                return $"{yearMonth}001";
            }

            string lastSequenceStr = lastTenant.TenantAccountNumber.Substring(6, 3);
            if (int.TryParse(lastSequenceStr, out int lastSequence))
            {
                int newSequence = lastSequence + 1;
                return $"{yearMonth}{newSequence.ToString("D3")}";
            }

            // Fallback in case of parsing errors
            return $"{yearMonth}001";
        }
    }
}
