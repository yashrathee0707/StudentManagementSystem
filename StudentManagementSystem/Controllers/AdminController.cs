using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDatabaseContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDatabaseContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create a new Student
        [HttpPost("createStudent")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // First, create the User in the Users table
                    var userResult = await _context.RegisterUserAsync(model.Email, model.Password, model.Email, "Student");

                    if (userResult != "User registration successful")
                    {
                        return BadRequest(userResult);
                    }

                    // Then, get the UserID (assuming the UserID is generated in the Users table)
                    var user = await _context.Users
                        .Where(u => u.Email == model.Email)
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return BadRequest("User creation failed.");
                    }

                    // Insert the Student using the InsertStudent stored procedure
                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "EXEC dbo.InsertStudent @FirstName, @LastName, @DateOfBirth, @Email, @EnrollmentDate, @UserID",
                        new SqlParameter("@FirstName", model.FirstName),
                        new SqlParameter("@LastName", model.LastName),
                        new SqlParameter("@DateOfBirth", model.DateOfBirth),
                        new SqlParameter("@Email", model.Email),
                        new SqlParameter("@EnrollmentDate", model.EnrollmentDate),
                        new SqlParameter("@UserID", user.UserID)
                    );

                    return Ok("Student created successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating student: {ex.Message}");
                    return BadRequest($"Error occurred: {ex.Message}");
                }
            }
            return BadRequest(ModelState);
        }

        // Create a new Professor
        [HttpPost("createProfessor")]
        public async Task<IActionResult> CreateProfessor([FromBody] CreateProfessorUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // First, create the User in the Users table
                    var userResult = await _context.RegisterUserAsync(model.Email, model.Password, model.Email, "Professor");

                    if (userResult != "User registration successful")
                    {
                        return BadRequest(userResult);
                    }

                    // Then, get the UserID (assuming the UserID is generated in the Users table)
                    var user = await _context.Users
                        .Where(u => u.Email == model.Email)
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return BadRequest("User creation failed.");
                    }

                    // Insert the Professor using the InsertProfessor stored procedure
                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "EXEC dbo.InsertProfessor @FirstName, @LastName, @Email, @HireDate, @UserID",
                        new SqlParameter("@FirstName", model.FirstName),
                        new SqlParameter("@LastName", model.LastName),
                        new SqlParameter("@Email", model.Email),
                        new SqlParameter("@HireDate", model.HireDate),
                        new SqlParameter("@UserID", user.UserID)
                    );

                    return Ok("Professor created successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating professor: {ex.Message}");
                    return BadRequest($"Error occurred: {ex.Message}");
                }
            }
            return BadRequest(ModelState);
        }

        // Get all users, excluding Admins
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                // Here, you could check the role based on your logic or use a stored procedure to get roles
                if (user.Role != "Admin") 
                {
                    userRolesViewModel.Add(new UserRolesViewModel
                    {
                        User = user,
                        Roles = new List<string> { user.Role }
                    });
                }
            }

            return Ok(userRolesViewModel);
        }

        // Delete a user by their ID
        [HttpPost("deleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] int userId) 
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Prevent admins from deleting themselves
            if (user.Role == "Admin")
            {
                return BadRequest("Admins cannot delete themselves.");
            }

            // Delete the User using stored procedure or direct approach
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.DeleteUser @UserID",
                new SqlParameter("@UserID", user.UserID)
            );

            return Ok("User deleted successfully.");
        }
    }
}
