using Rentora.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.DTOs
{
    public class UpdateComplaintStatusRequest
    {
        public ComplaintStatus Status { get; set; }
        public string? AdminResponse { get; set; }
    }
}
