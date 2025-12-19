namespace ChurchAttendance.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}




