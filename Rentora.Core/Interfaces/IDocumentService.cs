using Rentora.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Interfaces
{
    public interface IDocumentService
    {
        Task<(bool IsSuccess, string ErrorMessage)> UploadDocumentAsync(UploadDocumentRequest request, string fileUrl);
        Task<IEnumerable<DocumentResponseDto>> GetDocumentsByTenantAsync(string tenantId);
    }
}
