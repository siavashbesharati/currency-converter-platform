using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Api.Auth;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
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
            if (req.Username == "user" && req.Password == "password")
            {
                var token = _tokenService.GenerateToken(req.Username, new[] { "User" });
                return Ok(new { access_token = token });
            }

            return Unauthorized();
        }
    }

    public record LoginRequest(string Username, string Password);
}
