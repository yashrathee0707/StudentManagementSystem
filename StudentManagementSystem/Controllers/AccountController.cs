using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private static readonly List<User> Users = new List<User>
        {
            new User { UserName = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") }
        };

        private readonly string _secretKey = "0Bnx5Gfz5XmRsRZ6HzrZZ4eqRfAzD2+jJjUJevCBNkM=";

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Users.Any(u => u.UserName == model.Username))
                {
                    ModelState.AddModelError(string.Empty, "Username already exists");
                    return BadRequest(ModelState);
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                Users.Add(new User { UserName = model.Username, PasswordHash = hashedPassword });
                return Ok("Registration successful");
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Users.FirstOrDefault(u => u.UserName == model.Username);
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

        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Ok("Logout successful");
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}