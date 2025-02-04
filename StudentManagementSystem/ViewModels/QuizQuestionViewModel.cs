namespace StudentManagementSystem.ViewModels
{
    public class QuizQuestionViewModel
    {
        public int QuizQuestionID { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
    }
}
