namespace StudentManagementSystem.Models
{
    public class Project
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int CourseID { get; set; }
        public Course Course { get; set; }
    }
}