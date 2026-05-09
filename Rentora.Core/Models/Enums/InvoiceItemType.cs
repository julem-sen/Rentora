using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models.Enums
{
    public enum InvoiceItemType
    {
        Rent = 1,
        Maintenance = 2,
        Utility = 3,
        LateFee = 4,
        Other  = 5
    }
}
