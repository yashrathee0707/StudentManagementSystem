namespace StudentManagementSystem.ViewModels
{
    public class ProjectFileUploadModel
    {
        public int ProjectID { get; set; }
        public int StudentID { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
