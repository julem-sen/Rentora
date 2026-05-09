using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.DTOs
{
    public class SubmitComplaintRequest
    {
        public string TenantId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
    }
}
