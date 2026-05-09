using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;

namespace Rentora.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IFileStorageService _fileStorageService;

        public PaymentController(IPaymentService paymentService, IFileStorageService fileStorageService)
        {
            _paymentService = paymentService;
            _fileStorageService = fileStorageService;
        }

        // Endpoint for the Tenant Mobile App
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitPayment([FromForm] SubmitPaymentRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string? proofUrl = null;

            // 1. If the tenant uploaded an image, save it using our File Storage Service
            if (request.ReceiptImage != null)
            {
                proofUrl = await _fileStorageService.SaveFileAsync(request.ReceiptImage, "receipts");
            }
            else if (request.Method != Rentora.Core.Models.Enums.PaymentMethod.Cash)
            {
                // Force an image upload if they claim they paid via GCash or Bank Transfer
                return BadRequest("Proof of payment image is required for digital transfers.");
            }

            // 2. Pass the generated URL and data to the Payment Service
            var result = await _paymentService.SubmitPaymentAsync(
                request.InvoiceId,
                request.TenantId,
                request.AmountPaid,
                request.Method,
                proofUrl);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Payment submitted successfully and is pending verification." });
            }

            return BadRequest(new { Message = result.ErrorMessage });
        }

        // Endpoint for the Admin Web Dashboard
        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyPayment(int id, [FromBody] VerifyPaymentRequest request)
        {
            var result = await _paymentService.VerifyPaymentAsync(id, request.AdminNotes);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Payment verified successfully. Invoice marked as Paid." });
            }

            return BadRequest(new { Message = result.ErrorMessage });
        }
    }
}
