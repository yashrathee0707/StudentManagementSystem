namespace StudentManagementSystem.Models
{
    public class Student
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public int UserID { get; set; }

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<ProjectFile> ProjectFiles { get; set; } = new List<ProjectFile>();
        //public ICollection<QuizSubmission> QuizSubmissions { get; set; } = new List<QuizSubmission>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<StudentDiscipline> StudentDisciplines { get; set; } = new List<StudentDiscipline>();
        public ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();
    }
}