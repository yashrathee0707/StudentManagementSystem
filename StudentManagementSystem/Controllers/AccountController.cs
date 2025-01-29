using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private static readonly List<User> Users = new List<User>
        {
            new User { UserName = "admin", PasswordHash = "password" }
        };

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Ok("Login endpoint");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Users.FirstOrDefault(u => u.UserName == model.Username && u.PasswordHash == model.Password);
                if (user != null)
                {
                    var token = "dummy-token";
                    return Ok(new { Token = token });
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return BadRequest(ModelState);
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return Ok("Register endpoint");
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Handle registration logic here
                return Ok("Registration successful");
            }
            return BadRequest(ModelState);
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Handle logout logic here
            return Ok("Logout successful");
        }
    }
}