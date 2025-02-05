namespace StudentManagementSystem.Models
{
    public class Quiz 
    {
        public int QuizID { get; set; }
        public string QuizName { get; set; }

        public ICollection<QuizQuestion> QuizQuestions { get; set; }
    }
}
