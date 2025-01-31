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
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<StudentDiscipline> StudentDisciplines { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public int UserID { get; set; }


    }
}
