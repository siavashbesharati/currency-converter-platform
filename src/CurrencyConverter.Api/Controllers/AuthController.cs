using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Api.Auth;

using Microsoft.AspNetCore.RateLimiting;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [EnableRateLimiting("fixed")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // Very simple in-memory validation for demo/interview
            if (req.Username == "demo" && req.Password == "demo")
            {
                var token = _tokenService.GenerateToken(req.Username, new[] { "User" });
                return Ok(new { access_token = token });
            }

            return Unauthorized();
        }
    }

    public record LoginRequest(string Username, string Password);
}
