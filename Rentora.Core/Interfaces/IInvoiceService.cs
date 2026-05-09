using Rentora.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Interfaces
{
    public interface IInvoiceService
    {
        Task CreateMoveInInvoiceAsync(string tenantId, decimal monthlyRentAmount, int monthsAdvance, int monthsDeposit);
        Task CreateManualInvoiceAsync(CreateManualInvoiceRequest request);
    }
}
