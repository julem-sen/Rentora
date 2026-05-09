using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public InvoiceItemType Type { get; set; }
        public string Description { get; set; } // Incase not in the InvoiceItemType (Others)
        public decimal Amount { get; set; }

        // Navigation Property
        public Invoice Invoice { get; set; }
    }
}
