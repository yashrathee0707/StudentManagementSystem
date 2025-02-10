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
        public async Task<IActionResult> GetStudentProfile([FromQuery] int studentId)
        {
            if(studentId <= 0)
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

        // Edit student homework (GET)
        [HttpGet("editHomework/{id}")]
        public async Task<IActionResult> EditHomework(int id)
        {
            try
            {
                var homework = await _context.Assignments
                    .FromSqlRaw("EXEC dbo.GetHomeworkByID @HomeworkID",
                                new SqlParameter("@HomeworkID", id))
                    .ToListAsync(); 

                var result = homework.FirstOrDefault(); 

                if (result == null)
                {
                    throw new StudentControllerException($"Homework with ID {id} not found.");
                }

                return Ok(result);
            }
            catch (StudentControllerException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving homework: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Insert or update assignment (POST)
        [HttpPost("editHomework")]
        public async Task<IActionResult> EditHomework([FromBody] Assignment assignment)
        {
            if (assignment == null)
            {
                return BadRequest("Invalid assignment data.");
            }

            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.UpdateOrInsertAssignment @AssignmentID, @AssignmentName, @CourseID, @DueDate, @Title, @Description, @StudentID, @AssignmentType, @ProfessorID",
                    new SqlParameter("@AssignmentID", assignment.AssignmentID),
                    new SqlParameter("@AssignmentName", assignment.AssignmentName),
                    new SqlParameter("@CourseID", assignment.CourseID),
                    new SqlParameter("@DueDate", assignment.DueDate),
                    new SqlParameter("@Title", assignment.Title),
                    new SqlParameter("@Description", assignment.Description),
                    new SqlParameter("@StudentID", assignment.StudentID),
                    new SqlParameter("@AssignmentType", assignment.AssignmentType),
                    new SqlParameter("@ProfessorID", assignment.ProfessorID)
                );

                if (result == 0)
                {
                    return NotFound("Failed to update or insert assignment.");
                }

                return Ok("Assignment updated or inserted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating or inserting assignment: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
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

        // Get and submit quiz
        [HttpGet("quiz")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            if(quizId <= 0)
            {
                return BadRequest("Invalid quiz ID");
            }
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
            public StudentControllerException(string message) : base(message)
            {
            }
        }

    }
}