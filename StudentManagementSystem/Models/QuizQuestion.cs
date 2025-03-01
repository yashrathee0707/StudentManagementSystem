﻿using System.Text.Json.Serialization;

namespace StudentManagementSystem.Models
{
    public class QuizQuestion
    {
        public int QuizQuestionID { get; set; }
        public int QuizID { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }

        [JsonIgnore]
        public Quiz Quiz { get; set; }
    }
}