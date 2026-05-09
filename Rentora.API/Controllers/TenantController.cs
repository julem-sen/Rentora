using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;
using Rentora.Core.Models;
using Rentora.Infrastructure.Services;

namespace Rentora.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantSvc;

        public TenantController(ITenantService tenantSvc)
        {
            _tenantSvc = tenantSvc;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Hand off to the Service Layer
            var result = await _tenantSvc.CreateTenantAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Message = "Tenant and Move-In Invoice created successfully.",
                    TenantAccountNumber = result.AccountNumber
                });
            }

            // Return the cleanly formatted errors from the service
            return BadRequest(result.Errors);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTenants()
        {
            // Calling the service
            var result = await _tenantSvc.GetAllTenants();

            // Fix: Return the 'result' variable from the line above
            return Ok(result);
        }
    }
}
