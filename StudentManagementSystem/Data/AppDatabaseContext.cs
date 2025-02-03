﻿using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

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

        // Register User with Stored Procedure
        public async Task<string> RegisterUserAsync(string username, string passwordHash, string email, string role)
        {
            var parameters = new[] {
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
            var parameters = new[] {
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
            var parameters = new[] {
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
            var parameters = new[] {
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

            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Professor>().ToTable("Professors");
            modelBuilder.Entity<Course>().ToTable("Courses");

            modelBuilder.Entity<Assignment>()
                .HasDiscriminator<string>("AssignmentType")
                .HasValue<Assignment>("Assignment")
                .HasValue<Project>("Project")
                .HasValue<Quiz>("Quiz");

            modelBuilder.Entity<StudentDiscipline>().HasKey(sd => new { sd.StudentID, sd.DisciplineID });

            modelBuilder.Entity<Student>()
                .HasMany(e => e.Enrollments)
                .WithOne(e => e.Student)
                .HasForeignKey(e => e.StudentID);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Enrollments)
                .WithOne(e => e.Course)
                .HasForeignKey(e => e.CourseID);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Assignments)
                .WithOne(a => a.Course)
                .HasForeignKey(a => a.CourseID);

            modelBuilder.Entity<Assignment>()
                .HasMany(a => a.Submissions)
                .WithOne(s => s.Assignment)
                .HasForeignKey(s => s.AssignmentID);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.ProjectFiles)
                .WithOne(pf => pf.Project)
                .HasForeignKey(pf => pf.ProjectID);

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.QuizQuestions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.QuizID);

            //modelBuilder.Entity<StudentDiscipline>()
            //    .HasOne(sd => sd.Student)
            //    .WithMany(s => s.StudentDisciplines)
            //    .HasForeignKey(sd => sd.StudentID);

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
