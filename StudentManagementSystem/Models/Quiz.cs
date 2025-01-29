using System.Collections.Generic;

namespace StudentManagementSystem.Models
{
    public class Quiz : Assignment
    {
        public ICollection<QuizQuestion> QuizQuestions { get; set; }
    }
}