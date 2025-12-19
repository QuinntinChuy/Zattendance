namespace ChurchAttendance.ViewModels
{
    public class AddGroupNameViewModel
    {
        public string GroupName { get; set; } = string.Empty;
        public int? LeaderId { get; set; }
        public List<int> SelectedMembers { get; set; } = new List<int>();
    }
}


