using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Rentora.Core.DTOs;
using Rentora.Core.Interfaces;
using Rentora.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rentora.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Message = "Login successful",
                    Token = result.Token,
                    User = result.UserData
                });
            }

            return Unauthorized(new { Message = result.ErrorMessage });
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterAdminAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Admin account created successfully. You can now log in." });
            }

            return BadRequest(new { Message = result.ErrorMessage });
        }
    }
}
