namespace StudentManagementSystem.Models
{
    public class Submission
    {
        public int SubmissionID { get; set; }
        public int AssignmentID { get; set; }
        public int StudentID { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int? Grade { get; set; }

        public string Content { get; set; }  

        public Assignment Assignment { get; set; }
        public Student Student { get; set; }
    }
}
