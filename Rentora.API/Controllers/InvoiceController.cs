using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;


namespace Rentora.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("manual")]
        public async Task<IActionResult> CreateManualInvoice([FromBody] CreateManualInvoiceRequest request)
        {
            if (!ModelState.IsValid || !request.Items.Any())
            {
                return BadRequest("Invalid request or no items provided.");
            }

            // Pass the DTO straight to the service
            await _invoiceService.CreateManualInvoiceAsync(request);

            return Ok(new { Message = "Manual invoice created successfully." });
        }
    }
}
