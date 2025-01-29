using System.Collections.Generic;

namespace StudentManagementSystem.Models
{
    public class Project : Assignment
    {
        public ICollection<ProjectFile> ProjectFiles { get; set; }
    }
}