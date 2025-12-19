namespace ChurchAttendance.ViewModels
{
    public class AddMemberViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string? LifeNumber { get; set; } // Member Number / Life Number
        public string Gender { get; set; } = string.Empty; // Changed to string for JSON binding
        public int GroupId { get; set; }
        public string? Position { get; set; }
    }
}




