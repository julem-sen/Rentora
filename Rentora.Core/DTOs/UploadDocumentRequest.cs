using Microsoft.AspNetCore.Http;
using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.DTOs
{
    public class UploadDocumentRequest
    {
        public string TenantId { get; set; }
        public int? InvoiceId { get; set; } // Leave null if it's just a general document
        public string Title { get; set; }
        public DocumentType Type { get; set; }

        // The actual physical file (PDF or Image)
        public IFormFile File { get; set; }
    }
}
