using Microsoft.AspNetCore.Identity;

namespace ChurchAttendance.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public enum UserRole
    {
        Administrator,
        TeamLeader
    }
}




