using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AppDatabaseContext _context;

        public AccountController(AppDatabaseContext context)
        {
            _context = context;
        }

        // Registration endpoint
        // POST: /api/Account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == model.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Username already exists");
                    return BadRequest(ModelState);
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var role = model.Role ?? "User";

                var result = await _context.RegisterUserAsync(model.Username, hashedPassword, model.Email, role);

                if (result == "User registration successful")
                {
                    return Ok("Registration successful");
                }
                else
                {
                    return BadRequest(result); 
                }
            }

            return BadRequest(ModelState);
        }

        // Login endpoint
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == model.Username);
                if (user == null)
                {
                    return BadRequest("User not found. Please register first.");
                }

                if (BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new { Token = token });
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return BadRequest(ModelState);
        }

        // Logout endpoint (requires authentication)
        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Ok("Logout successful");
        }

        // Method to generate JWT token
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes("0Bnx5Gfz5XmRsRZ6HzrZZ4eqRfAzD2+jJjUJevCBNkM=");
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
