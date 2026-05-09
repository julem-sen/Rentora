using Rentora.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Token, object UserData, string ErrorMessage)> LoginAsync(LoginRequest request);
        Task<(bool IsSuccess, string ErrorMessage)> RegisterAdminAsync(RegisterAdminRequest request);
    }
}
