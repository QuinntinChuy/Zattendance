namespace ChurchAttendance.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public GroupType Type { get; set; }
        public Gender? GenderRestriction { get; set; }
        public string? Description { get; set; }
        
        public ICollection<Member> Members { get; set; } = new List<Member>();
    }

    public enum GroupType
    {
        Adult,
        YoungAdult,
        Children
    }
}




