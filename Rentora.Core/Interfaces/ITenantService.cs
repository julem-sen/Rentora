using Rentora.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Interfaces
{
    public interface ITenantService
    {
        //Task<string> GenerateTenantAccountNumberAsync();
        Task<(bool IsSuccess, string AccountNumber, IEnumerable<string> Errors)> CreateTenantAsync(CreateTenantRequest request);
        Task<IEnumerable<object>> GetAllTenants();
    }
}
