namespace StudentManagementSystem.ViewModels
{
    public class HomeworkEditModel
    {
        public int AssignmentID { get; set; }
        public int StudentID { get; set; }
        public string Content { get; set; }
        public int? Grade { get; set; }
    }
}
