using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public int InvoiceId { get; set; }
        public string Title { get; set; }
        public string DocumentUrl { get; set; } // Path to the stored document file
        public DocumentType Type { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.Now; // Timestamp of when the document was uploaded

        // Navigation Properties
        public ApplicationUser Tenant { get; set; } // Navigation property to the tenant
        public Invoice Invoice { get; set; } // Navigation property to the associated invoice
    }
}
