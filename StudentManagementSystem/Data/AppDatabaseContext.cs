using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using StudentManagementSystem.ViewModels;

namespace StudentManagementSystem.Data
{
    public class AppDatabaseContext : DbContext
    {
        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Professor> Professors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectFile> ProjectFiles { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<StudentDiscipline> StudentDisciplines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<QuizAssignment> QuizAssignments { get; set; }

        // Register User with Stored Procedure
        public async Task<string> RegisterUserAsync(string username, string passwordHash, string email, string role)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = username },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar) { Value = passwordHash },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email },
                new SqlParameter("@Role", SqlDbType.NVarChar) { Value = role }
            };

            try
            {
                var result = await this.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_InsertUser @UserName, @PasswordHash, @Email, @Role",
                    parameters);

                return "User registration successful";
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return "Username already exists";
                }

                return $"Error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Unexpected error occurred: {ex.Message}";
            }
        }

        // Create Student using Stored Procedure
        public async Task<string> CreateStudentAsync(string firstName, string lastName, DateTime dateOfBirth, string email, DateTime enrollmentDate, int userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = firstName },
                new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = lastName },
                new SqlParameter("@DateOfBirth", SqlDbType.DateTime) { Value = dateOfBirth },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email },
                new SqlParameter("@EnrollmentDate", SqlDbType.DateTime) { Value = enrollmentDate },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
            };

            try
            {
                await this.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.InsertStudent @FirstName, @LastName, @DateOfBirth, @Email, @EnrollmentDate, @UserID",
                    parameters);

                return "Student created successfully.";
            }
            catch (Exception ex)
            {
                return $"Error occurred: {ex.Message}";
            }
        }

        // Create Professor using Stored Procedure
        public async Task<string> CreateProfessorAsync(string firstName, string lastName, string email, DateTime hireDate, int userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = firstName },
                new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = lastName },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email },
                new SqlParameter("@HireDate", SqlDbType.DateTime) { Value = hireDate },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
            };

            try
            {
                await this.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.InsertProfessor @FirstName, @LastName, @Email, @HireDate, @UserID",
                    parameters);

                return "Professor created successfully.";
            }
            catch (Exception ex)
            {
                return $"Error occurred: {ex.Message}";
            }
        }

        // Delete User using Stored Procedure
        public async Task<string> DeleteUserAsync(int userId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
            };

            try
            {
                await this.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.DeleteUser @UserID",
                    parameters);

                return "User deleted successfully.";
            }
            catch (Exception ex)
            {
                return $"Error occurred: {ex.Message}";
            }
        }

        // Method to get Student Profile
        public async Task<Student> GetStudentProfileAsync(int studentId)
        {
            return await this.Students
                .FromSqlRaw("EXEC dbo.GetStudentProfile @StudentID",
                            new SqlParameter("@StudentID", studentId))
                .FirstOrDefaultAsync();
        }

        // Method to get Student Homework
        public async Task<List<Assignment>> GetStudentHomeworkAsync(int studentId)
        {
            return await this.Assignments
                .FromSqlRaw("EXEC dbo.GetStudentHomework @StudentID",
                            new SqlParameter("@StudentID", studentId))
                .ToListAsync();
        }

        public async Task<HomeworkViewModel> GetHomeworkByIdAsync(int homeworkId)
        {
            var result = await Task.Run(() => this.Assignments
                .FromSqlRaw("EXEC dbo.GetHomeworkById @HomeworkID", new SqlParameter("@HomeworkID", homeworkId))
                .AsEnumerable() // Perform the composition on the client side
                .GroupBy(a => a.AssignmentID) // Group by a unique field
                .Select(g => g.First()) // Select the first item in each group
                .Select(a => new HomeworkViewModel
                {
                    Id = a.AssignmentID,
                    Title = a.Title,
                    Content = a.Description,
                    EndDate = a.DueDate,
                    Comment = "",
                    Mandatory = false,
                    Penalty = 0,
                    AfterEndUploadDate = a.DueDate
                })
                .GroupBy(a => a.Content) // Group by description to ensure uniqueness
                .Select(g => g.First()) // Select the first item in each group
                .FirstOrDefault());

            return result;
        }





        // Method to Update Homework
        public async Task<int> UpdateHomeworkAsync(int homeworkId, string content)
        {
            return await this.Database.ExecuteSqlRawAsync(
                "EXEC dbo.UpdateHomework @HomeworkID, @Content",
                new SqlParameter("@HomeworkID", homeworkId),
                new SqlParameter("@Content", content)
            );
        }

        // Method to Upload Project Files
        public async Task<int> UploadProjectFileAsync(int projectId, int studentId, string fileName, byte[] fileContent)
        {
            return await this.Database.ExecuteSqlRawAsync(
                "EXEC dbo.UploadProjectFile @ProjectID, @StudentID, @FileName, @FileContent",
                new SqlParameter("@ProjectID", projectId),
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@FileName", fileName),
                new SqlParameter("@FileContent", fileContent)
            );
        }

        // Method to Get Quiz Questions
        public async Task<List<QuizQuestion>> GetQuizQuestionsAsync(int quizId)
        {
            return await this.QuizQuestions
                .FromSqlRaw("EXEC dbo.GetQuizQuestions @QuizID",
                            new SqlParameter("@QuizID", quizId))
                .ToListAsync();
        }

        // Method to Submit Quiz Answers
        public async Task<int> SubmitQuizAnswersAsync(int quizId, int quizQuestionId, int studentId, string answer)
        {
            return await this.Database.ExecuteSqlRawAsync(
                "EXEC dbo.SubmitQuizAnswers @QuizID, @QuizQuestionID, @StudentID, @Answer",
                new SqlParameter("@QuizID", quizId),
                new SqlParameter("@QuizQuestionID", quizQuestionId),
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@Answer", answer)
            );
        }

        // Method to get Student Assignments
        public async Task<List<Assignment>> GetStudentAssignmentsAsync(int studentId)
        {
            try
            {
                var assignments = await this.Set<Assignment>().FromSqlRaw(
                    "EXEC dbo.GetStudentAssignments @StudentID = {0}", studentId).ToListAsync();
                return assignments;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching assignments: {ex.Message}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Assignment entity
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.AssignmentID);
                entity.Property(e => e.AssignmentName).IsRequired();
                entity.Property(e => e.CourseID).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.StudentID).IsRequired();
                entity.Property(e => e.AssignmentType).IsRequired();
                //entity.Property(e => e.ProfessorID).IsRequired();
            });

            // Configure Quiz entity
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.QuizID);
                entity.Property(e => e.QuizName).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.CourseID).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.HasMany(e => e.QuizQuestions)
                      .WithOne(q => q.Quiz)
                      .HasForeignKey(q => q.QuizID);
            });

            // Configure QuizAssignment entity
            modelBuilder.Entity<QuizAssignment>(entity =>
            {
                entity.HasKey(e => new { e.QuizID, e.StudentID });
                entity.HasOne(e => e.Quiz)
                      .WithMany(q => q.QuizAssignments)
                      .HasForeignKey(e => e.QuizID);
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.QuizAssignments)
                      .HasForeignKey(e => e.StudentID);
            });

            // Configure QuizQuestion entity
            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.HasKey(e => e.QuizQuestionID);
                entity.Property(e => e.QuestionText).IsRequired();
                entity.Property(e => e.CorrectAnswer).IsRequired();
                entity.HasOne(e => e.Quiz)
                      .WithMany(q => q.QuizQuestions)
                      .HasForeignKey(e => e.QuizID);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseID);
                entity.Property(e => e.CourseName).IsRequired();
                //entity.Property(e => e.ProfessorID).IsRequired();  
            });

            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Professor>().ToTable("Professors");
            modelBuilder.Entity<Course>().ToTable("Courses");

            // Many-to-many relationship between Student and Discipline
            modelBuilder.Entity<StudentDiscipline>().HasKey(sd => new { sd.StudentID, sd.DisciplineID });

            // Relationships
            modelBuilder.Entity<Student>()
                .HasMany(e => e.Enrollments)
                .WithOne(e => e.Student)
                .HasForeignKey(e => e.StudentID);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Enrollments)
                .WithOne(e => e.Course)
                .HasForeignKey(e => e.CourseID);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.ProjectFiles)
                .WithOne(pf => pf.Project)
                .HasForeignKey(pf => pf.ProjectID);

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.QuizQuestions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.QuizID);

            modelBuilder.Entity<StudentDiscipline>()
                .HasOne(sd => sd.Discipline)
                .WithMany(d => d.StudentDisciplines)
                .HasForeignKey(sd => sd.DisciplineID);

            modelBuilder.Entity<Professor>()
                .HasMany(p => p.Courses)
                .WithOne(c => c.Professor)
                .HasForeignKey(c => c.ProfessorID);

            modelBuilder.Entity<Student>()
                .HasMany(s => s.Submissions)
                .WithOne(sub => sub.Student)
                .HasForeignKey(sub => sub.StudentID);
        }
    }
}