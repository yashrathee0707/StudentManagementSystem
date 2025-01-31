using System;

namespace StudentManagementSystem.Models
{
    public class CreateProfessorUserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public string Password { get; set; } 
    }
}
