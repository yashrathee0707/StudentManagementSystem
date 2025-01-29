namespace StudentManagementSystem.Models
{
    public class Discipline
    {
        public int DisciplineID { get; set; }
        public string DisciplineName { get; set; }
        public ICollection<StudentDiscipline> StudentDisciplines { get; set; } 
    }
}