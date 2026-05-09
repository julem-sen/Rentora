using Rentora.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Interfaces
{
    public interface IComplaintService
    {
        Task<(bool IsSuccess, string ErrorMessage)> SubmitComplaintAsync(SubmitComplaintRequest request);
        Task<(bool IsSuccess, string ErrorMessage)> UpdateComplaintStatusAsync(int complaintId, UpdateComplaintStatusRequest request);
    }
}
