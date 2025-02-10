namespace StudentManagementSystem.Models
{
    public class QuizAssignment
    {
        public int QuizID { get; set; }
        public int StudentID { get; set; }

        public Quiz Quiz { get; set; }
        public Student Student { get; set; }
    }
}