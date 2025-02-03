namespace StudentManagementSystem.Models
{
    public class Quiz : Assignment
    {
        public int QuizID { get; set; }  // Add QuizID if you need a separate identifier
        public ICollection<QuizQuestion> QuizQuestions { get; set; }
    }
}
