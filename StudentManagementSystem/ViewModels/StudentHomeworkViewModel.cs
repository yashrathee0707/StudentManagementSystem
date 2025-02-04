namespace StudentManagementSystem.ViewModels
{
    public class StudentHomeworkViewModel
    {
        public int HomeworkID { get; set; }
        public string HomeworkName { get; set; }
        public DateTime DueDate { get; set; }
        public string Content { get; set; }
    }
}