using ChurchAttendance.Models;

namespace ChurchAttendance.ViewModels
{
    public class AttendanceIndexViewModel
    {
        public Schedule? Schedule { get; set; }
        public List<Member> Members { get; set; } = new();
        public List<Attendance> Attendances { get; set; } = new();
        public List<Schedule> AllSchedules { get; set; } = new();
        public List<Group> AllGroups { get; set; } = new();
        public List<string> AllPositions { get; set; } = new();
        public int? FilterGroupId { get; set; }
        public string? FilterPosition { get; set; }
    }
}




