namespace StudentManagementSystem.ViewModels
{
    public class QuizSubmissionModel
    {
        public int QuizID { get; set; }
        public int StudentID { get; set; }
        public decimal Grade { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}
