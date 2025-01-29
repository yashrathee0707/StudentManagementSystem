namespace StudentManagementSystem.Models
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public string QuizName { get; set; }
        public int CourseID { get; set; }
        public Course Course { get; set; }
    }
}