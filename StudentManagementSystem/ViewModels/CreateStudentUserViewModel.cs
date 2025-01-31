using System;

namespace StudentManagementSystem.Models
{
    public class CreateStudentUserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Password { get; set; }
    }
}
