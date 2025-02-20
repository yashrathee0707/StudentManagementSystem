using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Student, Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDatabaseContext _context;
        private readonly ILogger<StudentController> _logger;

        public StudentController(AppDatabaseContext context, ILogger<StudentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get student profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetStudentProfile([FromQuery] int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest("Invalid student ID");
            }
            try
            {
                var students = _context.Students
                    .FromSqlRaw("EXEC GetStudentByID @StudentId = {0}", studentId)
                    .AsEnumerable();

                var student = students.FirstOrDefault();

                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching student profile: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Get student homework
        [HttpGet("homework")]
        public async Task<IActionResult> GetHomework(int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest("Invalid student ID");
            }
            try
            {
                var homework = await _context.GetStudentHomeworkAsync(studentId);

                return Ok(homework);
            }
            catch (InvalidCastException ex)
            {
                _logger.LogError($"Invalid cast exception: {ex.Message}");
                return StatusCode(500, "Internal server error: Invalid cast exception.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching homework: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/Student/EditHomework/{id}
        [HttpGet("EditHomework/{id}")]
        public async Task<IActionResult> EditHomework(string id)
        {
            if (!int.TryParse(id, out var homeworkId))
            {
                return BadRequest("Invalid homework ID");
            }

            try
            {
                var homework = await _context.GetHomeworkByIdAsync(homeworkId);
                if (homework == null)
                {
                    // Exception thrown if homework is not found
                    throw new StudentControllerException("Homework not found", 404);
                }

                return Ok(homework);
            }
            catch (StudentControllerException)
            {
                ModelState.AddModelError("", "An error occurred, changes were not saved.");
                return NotFound("Homework not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching homework: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/Student/EditHomework
        [HttpPost("EditHomework")]
        public async Task<IActionResult> EditHomework(HomeworkViewModel homework)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _context.UpdateHomeworkAsync(homework.Id, homework.Content);
                    if (result == 0)
                    {
                        return NotFound();
                    }
                    return RedirectToAction(nameof(GetHomework), new { studentId = homework.Id });
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "An error occurred, changes were not saved.");
                }
            }

            return BadRequest(ModelState);
        }

        // Upload project files
        [HttpPost("uploadProjectFiles")]
        public async Task<IActionResult> UploadProjectFiles([FromQuery] int projectId, [FromQuery] int studentId, IFormFile file)
        {
            if (projectId <= 0 || studentId <= 0)
            {
                return BadRequest("Invalid project ID or student ID.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file.");
            }

            if (file.Length > 10 * 1024 * 1024) // 10 MB limit
            {
                return BadRequest("File size exceeds the 10 MB limit.");
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileContent = memoryStream.ToArray();

                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "EXEC dbo.UploadProjectFile @ProjectID, @StudentID, @FileName, @FileContent",
                        new SqlParameter("@ProjectID", projectId),
                        new SqlParameter("@StudentID", studentId),
                        new SqlParameter("@FileName", file.FileName),
                        new SqlParameter("@FileContent", fileContent)
                    );

                    if (result == 0)
                    {
                        return BadRequest("Failed to upload project file.");
                    }

                    return Ok("Project file uploaded successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading project file: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Get quizzes assigned to a student
        [HttpGet("quiz")]
        public async Task<IActionResult> GetQuiz(int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest("Invalid student ID");
            }
            try
            {
                var quizzes = _context.Quizzes
                    .FromSqlRaw("EXEC dbo.GetAssignedQuizzes @StudentID",
                                new SqlParameter("@StudentID", studentId))
                    .AsEnumerable()
                    .Select(q => _context.Quizzes
                        .Include(q => q.QuizQuestions)
                        .Include(q => q.QuizAssignments)
                        .FirstOrDefault(x => x.QuizID == q.QuizID))
                    .ToList();

                if (!quizzes.Any())
                {
                    return NotFound("No quizzes found for the student.");
                }

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching quizzes: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("submitQuiz")]
        public async Task<IActionResult> SubmitQuiz(int quizId, int studentId, [FromBody] Dictionary<int, string> answers)
        {
            if (quizId <= 0 || studentId <= 0)
            {
                return BadRequest("Invalid quiz ID or student ID.");
            }

            if (answers == null || answers.Count == 0)
            {
                return BadRequest("Answers cannot be empty.");
            }

            try
            {
                foreach (var answer in answers)
                {
                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "EXEC dbo.SubmitQuizAnswers @QuizID, @QuizQuestionID, @StudentID, @Answer",
                        new SqlParameter("@QuizID", quizId),
                        new SqlParameter("@QuizQuestionID", answer.Key),
                        new SqlParameter("@StudentID", studentId),
                        new SqlParameter("@Answer", answer.Value)
                    );

                    if (result == 0)
                    {
                        return BadRequest("Failed to submit quiz answers.");
                    }
                }

                return Ok("Quiz submitted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting quiz: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        public class StudentControllerException : Exception
        {
            public StudentControllerException(string message, int v) : base(message)
            {
            }
        }
    }
}