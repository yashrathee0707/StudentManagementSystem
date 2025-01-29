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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entities to tables
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Professor>().ToTable("Professors");
            modelBuilder.Entity<Course>().ToTable("Courses");

            // Define inheritance hierarchy for Assignment
            modelBuilder.Entity<Assignment>()
                .HasDiscriminator<string>("AssignmentType")
                .HasValue<Assignment>("Assignment")
                .HasValue<Project>("Project")
                .HasValue<Quiz>("Quiz");

            // Define composite keys
            modelBuilder.Entity<StudentDiscipline>().HasKey(sd => new { sd.StudentID, sd.DisciplineID });

            // Define relationships and constraints
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

            modelBuilder.Entity<StudentDiscipline>()
                .HasOne(sd => sd.Student)
                .WithMany(s => s.StudentDisciplines)
                .HasForeignKey(sd => sd.StudentID);

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