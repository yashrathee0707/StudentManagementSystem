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
        public async Task<IActionResult> GetProfile(int studentId)
        {
            try
            {
                var student = await _context.Students
                    .FromSqlRaw("EXEC dbo.GetStudentProfile @StudentID",
                                new SqlParameter("@StudentID", studentId))
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching student profile: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Get student homework
        [HttpGet("homework")]
        public async Task<IActionResult> GetHomework(int studentId)
        {
            try
            {
                var homework = await _context.Assignments
                    .FromSqlRaw("EXEC dbo.GetStudentHomework @StudentID",
                                new SqlParameter("@StudentID", studentId))
                    .ToListAsync();

                return Ok(homework);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching homework: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Edit student homework
        [HttpPost("editHomework")]
        public async Task<IActionResult> EditHomework(int homeworkId, [FromBody] string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("Content cannot be empty.");
            }

            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.UpdateHomework @HomeworkID, @Content",
                    new SqlParameter("@HomeworkID", homeworkId),
                    new SqlParameter("@Content", content)
                );

                if (result == 0)
                {
                    return BadRequest("Failed to update homework.");
                }

                return Ok("Homework updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating homework: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Upload project files
        [HttpPost("uploadProjectFiles")]
        public async Task<IActionResult> UploadProjectFiles(int projectId, int studentId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file.");
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

        // Get and submit quiz
        [HttpGet("quiz")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            try
            {
                var quizQuestions = await _context.QuizQuestions
                    .FromSqlRaw("EXEC dbo.GetQuizQuestions @QuizID",
                                new SqlParameter("@QuizID", quizId))
                    .ToListAsync();

                return Ok(quizQuestions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching quiz questions: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("submitQuiz")]
        public async Task<IActionResult> SubmitQuiz(int quizId, int studentId, [FromBody] Dictionary<int, string> answers)
        {
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
    }
}