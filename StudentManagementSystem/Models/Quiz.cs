using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudentManagementSystem.Models
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public string QuizName { get; set; }
        public string Description { get; set; }
        public int CourseID { get; set; }
        public DateTime DueDate { get; set; }
        public int ProfessorID { get; set; }

        public Course Course { get; set; }

        [JsonIgnore]
        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

        [JsonIgnore]
        public ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();
    }
}