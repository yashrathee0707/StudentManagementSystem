using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.ViewModels;
using Microsoft.Data.SqlClient;
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
        private readonly IConfiguration _configuration;

        public AccountController(AppDatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
                if (existingUser != null) return BadRequest("Username already exists");

                var allowedRoles = new List<string> { "Student", "Professor", "Admin" };
                var role = allowedRoles.Contains(model.Role) ? model.Role : "Student";

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                var result = await _context.RegisterUserAsync(model.Username, hashedPassword, model.Email, role);

                if (result == "User registration successful")
                {
                    if (role == "Student")
                        await AddStudentToAllDisciplinesAsync(model.Username);
                    return Ok("Registration successful");
                }
                return BadRequest(result);
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users
                    .Where(u => u.UserName == model.Username)
                    .Select(u => new { u.UserName, u.PasswordHash, u.Role })
                    .FirstOrDefault();

                if (user == null) return BadRequest("User not found. Please register first.");

                if (BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    var token = GenerateJwtToken(new User { UserName = user.UserName, Role = user.Role });
                    return Ok(new { Token = token });
                }

                return BadRequest("Invalid login attempt.");
            }
            return BadRequest(ModelState);
        }

        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return Ok("Logout successful");
        }

        private async Task AddStudentToAllDisciplinesAsync(string username)
        {
            try
            {
                // Retrieve the user based on the username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

                if (user == null)
                {
                    Console.WriteLine($"User with username {username} not found.");
                    return;
                }

                if (user.Role != "Student")
                {
                    Console.WriteLine($"User {username} is not a student. Skipping discipline assignment.");
                    return;
                }

                // Retrieve all disciplines
                var disciplines = await _context.Disciplines.ToListAsync();
                if (disciplines.Count == 0)
                {
                    Console.WriteLine("No disciplines found to assign the student.");
                    return;
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Iterate through each discipline and assign the student
                        foreach (var discipline in disciplines)
                        {
                            Console.WriteLine($"Assigning student {user.UserName} to discipline {discipline.DisciplineName}");

                            // Execute stored procedure for assigning the student to discipline
                            await _context.Database.ExecuteSqlRawAsync(
                                "EXEC dbo.sp_AddStudentToDiscipline @UserID, @DisciplineID",
                                new SqlParameter("@UserID", user.UserID),
                                new SqlParameter("@DisciplineID", discipline.DisciplineID)
                            );

                            Console.WriteLine($"Successfully assigned student {user.UserName} to discipline {discipline.DisciplineName}");
                        }

                        // Commit the transaction if all assignments are successful
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // Rollback in case of any failure
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error assigning student to disciplines: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding student {username} to disciplines: {ex.Message}");
            }
        }

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
