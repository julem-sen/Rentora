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
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IFileStorageService _fileStorageService;

        public DocumentController(IDocumentService documentService, IFileStorageService fileStorageService)
        {
            _documentService = documentService;
            _fileStorageService = fileStorageService;
        }

        // Only the Admin should be able to upload official documents!
        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            if (!ModelState.IsValid || request.File == null)
                return BadRequest("Invalid request or missing file.");

            // 1. Save the physical file using our secure storage service
            string fileUrl;
            try
            {
                fileUrl = await _fileStorageService.SaveFileAsync(request.File, "documents");
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message }); // Catch the 5MB or invalid extension errors
            }

            // 2. Save the record in the database
            var result = await _documentService.UploadDocumentAsync(request, fileUrl);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Document uploaded successfully." });
            }

            return BadRequest(new { Message = result.ErrorMessage });
        }

        // Tenants can call this to see all their documents
        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetTenantDocuments(string tenantId)
        {
            var documents = await _documentService.GetDocumentsByTenantAsync(tenantId);
            return Ok(documents);
        }
    }
}
