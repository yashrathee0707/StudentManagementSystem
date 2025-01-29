namespace StudentManagementSystem.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public int ProfessorID { get; set; } 
        public Professor Professor { get; set; } 
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
    }
}
