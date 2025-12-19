namespace ChurchAttendance.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public int GroupId { get; set; }
        public Group? Group { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? MemberNumber { get; set; } // For storing M05-xxxxx reference IDs
        
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum Gender
    {
        Male,
        Female
    }
}




