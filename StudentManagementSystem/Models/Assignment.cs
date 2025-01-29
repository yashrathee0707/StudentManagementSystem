namespace StudentManagementSystem.Models
{
    public class Assignment
    {
        public int AssignmentID { get; set; }
        public string AssignmentName { get; set; }
        public int CourseID { get; set; }
        public DateTime DueDate { get; set; }
        public Course Course { get; set; }
    }
}