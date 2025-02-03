namespace StudentManagementSystem.Models
{
    public class ProjectFile
    {
        public int ProjectFileID { get; set; }
        public int ProjectID { get; set; }
        public int StudentID { get; set; } 
        public string FileName { get; set; } 
        public byte[] FileContent { get; set; }  

        public DateTime UploadDate { get; set; }

        public Project Project { get; set; }
        public Student Student { get; set; }
    }
}
