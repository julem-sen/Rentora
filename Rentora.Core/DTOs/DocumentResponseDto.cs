using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.DTOs
{
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FileUrl { get; set; }
        public string Type { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
