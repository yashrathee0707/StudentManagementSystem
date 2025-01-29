namespace StudentManagementSystem.Models
{
    public class StudentDiscipline
    {
        public int StudentDisciplineID { get; set; }
        public int StudentID { get; set; }
        public int DisciplineID { get; set; }
        public Student Student { get; set; }
        public Discipline Discipline { get; set; }
    }
}