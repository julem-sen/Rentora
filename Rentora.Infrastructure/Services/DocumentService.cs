using Microsoft.EntityFrameworkCore;
using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;
using Rentora.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;

        public DocumentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> UploadDocumentAsync(UploadDocumentRequest request, string fileUrl)
        {
            var document = new Core.Models.Document
            {
                TenantId = request.TenantId,
                InvoiceId = request.InvoiceId ?? 0, // Use 0 if null
                Title = request.Title,
                Type = request.Type,
                DocumentUrl = fileUrl,
                UploadedDate = DateTime.Now
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        public async Task<IEnumerable<DocumentResponseDto>> GetDocumentsByTenantAsync(string tenantId)
        {
            return await _context.Documents
                .Where(d => d.TenantId == tenantId)
                .OrderByDescending(d => d.UploadedDate)
                .Select(d => new DocumentResponseDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileUrl = d.DocumentUrl,
                    Type = d.Type.ToString(),
                    UploadDate = d.UploadedDate
                })
                .ToListAsync();
        }
    }
}
