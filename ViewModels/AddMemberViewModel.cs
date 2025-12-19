using ChurchAttendance.Models;

namespace ChurchAttendance.ViewModels
{
    public class AddMemberViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public int GroupId { get; set; }
    }
}




