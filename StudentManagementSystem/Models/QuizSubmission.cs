namespace StudentManagementSystem.Models
{
    public class QuizSubmission
    {
        public int QuizSubmissionID { get; set; }
        public int QuizID { get; set; }
        public int StudentID { get; set; }
        public decimal Grade { get; set; }
        public DateTime SubmissionDate { get; set; }

        public Quiz Quiz { get; set; }
        public Student Student { get; set; }
    }
}
