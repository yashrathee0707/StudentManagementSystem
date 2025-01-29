namespace StudentManagementSystem.Models
{
    public class ProjectFile
    {
        public int ProjectFileID { get; set; }
        public int ProjectID { get; set; }
        public string FilePath { get; set; }
        public Project Project { get; set; }
    }
}