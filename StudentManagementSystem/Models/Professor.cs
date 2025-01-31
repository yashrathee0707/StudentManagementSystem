namespace StudentManagementSystem.Models
{
    public class Professor
    {
        public int ProfessorID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }
        public int UserID { get; set; }
    }
}
