﻿namespace StudentManagementSystem.Models
{
    public class HomeworkViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public DateTime EndDate { get; set; }
        public string Comment { get; set; }
        public bool Mandatory { get; set; }
        public int Penalty { get; set; }
        public DateTime AfterEndUploadDate { get; set; }
    }
}