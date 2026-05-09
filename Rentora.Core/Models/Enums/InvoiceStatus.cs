using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models.Enums
{
    public enum InvoiceStatus
    {
        Pending = 1,
        Paid = 2,
        Overdue = 3,
        RolledOver = 4,
        Cancelled = 5
    }
}
