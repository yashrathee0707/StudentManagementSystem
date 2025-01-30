using StudentManagementSystem.Models;
namespace StudentManagementSystem.ViewModels
{
    public class UserRolesViewModel
    {
        public User User { get; set; }
        public IList<string> Roles { get; set; }
    }
}