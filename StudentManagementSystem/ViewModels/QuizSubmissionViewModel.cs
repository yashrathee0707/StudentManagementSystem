using System.Collections.Generic;

namespace StudentManagementSystem.ViewModels
{
    public class QuizSubmissionViewModel
    {
        public int QuizID { get; set; } // ID of the quiz being submitted
        public List<QuizAnswerViewModel> Answers { get; set; } // List of answers provided by the student
    }

    public class QuizAnswerViewModel
    {
        public int QuestionID { get; set; } // ID of the question being answered
        public string Answer { get; set; } // The student's answer
    }
}
