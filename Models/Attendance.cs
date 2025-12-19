namespace ChurchAttendance.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member? Member { get; set; }
        public int ScheduleId { get; set; }
        public Schedule? Schedule { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }

    public enum AttendanceStatus
    {
        Present,
        Late,
        Absent
    }
}




