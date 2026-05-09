using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;
using Rentora.Core.Models;
using Rentora.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Infrastructure.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly ApplicationDbContext _context;

        public ComplaintService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> SubmitComplaintAsync(SubmitComplaintRequest request)
        {
            var complaint = new Complaint
            {
                TenantId = request.TenantId,
                Subject = request.Subject,
                Description = request.Description,
                DateSubmitted = DateTime.UtcNow,
                Status = Core.Models.Enums.ComplaintStatus.Pending
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> UpdateComplaintStatusAsync(int complaintId, UpdateComplaintStatusRequest request)
        {
            var complaint = await _context.Complaints.FindAsync(complaintId);

            if (complaint == null)
            {
                return (false, "Complaint not found.");
            }

            complaint.Status = request.Status;
            complaint.AdminResponse = request.AdminResponse;

            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }
    }
}
