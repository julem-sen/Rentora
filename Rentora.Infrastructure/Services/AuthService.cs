using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;
using Rentora.Core.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<(bool IsSuccess, string Token, object UserData, string ErrorMessage)> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username) ?? await _userManager.FindByEmailAsync(request.Username);

            if (user == null || !user.IsActive)
            {
                return (false, string.Empty, null, "Invalid credentials or inactive account.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                return (false, string.Empty, null, "Invalid credentials.");
            }

            var token = await GenerateJwtToken(user);

            var userData = new
            {
                user.FullName,
                user.TenantAccountNumber,
                user.UnitNumber
            };

            return (true, token, userData, string.Empty);
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Key"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var roles = await _userManager.GetRolesAsync(user);
            string userRole = roles.FirstOrDefault() ?? "Tenant";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("FullName", user.FullName),
                new Claim("UnitNumber", user.UnitNumber ?? ""),
                new Claim(ClaimTypes.Role, userRole)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> RegisterAdminAsync(RegisterAdminRequest request)
        {
            // 1. Check if ANY admin already exists in the entire database
            var existingAdmins = await _userManager.GetUsersInRoleAsync("Admin");

            if (existingAdmins.Any())
            {
                return (false, "An Admin account has already been setup. Please proceed to the login page.");
            }

            // 2. Create the Admin User
            var newAdmin = new ApplicationUser
            {
                UserName = request.Email, // Admins login with Email, Tenants login with Account Number
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(newAdmin, request.Password);

            if (result.Succeeded)
            {
                // 3. Assign the "Admin" role
                await _userManager.AddToRoleAsync(newAdmin, "Admin");
                return (true, string.Empty);
            }

            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
