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
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;

        public ComplaintController(IComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        // Endpoint for the Tenant to submit an issue
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitComplaint([FromBody] SubmitComplaintRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _complaintService.SubmitComplaintAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Complaint submitted successfully. The Admin has been notified." });
            }

            return BadRequest(new { Message = result.ErrorMessage });
        }

        // Endpoint for the Admin to update the status of the issue
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateComplaintStatusRequest request)
        {
            var result = await _complaintService.UpdateComplaintStatusAsync(id, request);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Complaint status updated successfully." });
            }

            return BadRequest(new { Message = result.ErrorMessage });
        }
    }
}
