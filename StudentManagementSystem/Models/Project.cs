using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Project 
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public ICollection<ProjectFile> ProjectFiles { get; set; }
        
    }
}