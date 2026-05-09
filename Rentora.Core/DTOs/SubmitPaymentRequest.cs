using Microsoft.AspNetCore.Http;
using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.DTOs
{
    public class SubmitPaymentRequest
    {
        public int InvoiceId { get; set; }
        public string TenantId { get; set; }
        public decimal AmountPaid { get; set; }
        public PaymentMethod Method { get; set; }

        public IFormFile? ReceiptImage { get; set; }
    }
}
