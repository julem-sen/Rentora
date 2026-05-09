using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Models
{
    public class RErrorLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }
        public string? RequestUrl { get; set; }
    }
}
