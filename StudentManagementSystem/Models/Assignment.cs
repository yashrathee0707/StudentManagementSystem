namespace StudentManagementSystem.Models
{
    public class Assignment
    {
        public int AssignmentID { get; set; }
        public string AssignmentName { get; set; }
        public int CourseID { get; set; }
        public DateTime DueDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StudentID { get; set; }
        public string AssignmentType { get; set; }
        public int ProfessorID { get; set; }

        //public Course Course { get; set; }
        //public ICollection<Submission> Submissions { get; set; }
        //public Student Student { get; set; }
        //public Submission Submission { get; set; }
    }
}