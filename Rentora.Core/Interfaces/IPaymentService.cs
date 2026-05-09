using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<(bool IsSuccess, string ErrorMessage)> SubmitPaymentAsync(int invoiceId, string tenantId, decimal amountPaid, PaymentMethod method, string? proofOfPaymentUrl);

        Task<(bool IsSuccess, string ErrorMessage)> VerifyPaymentAsync(int paymentId, string? adminNotes);
    }
}
