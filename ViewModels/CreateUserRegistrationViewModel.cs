namespace ChurchAttendance.ViewModels
{
    public class CreateUserRegistrationViewModel
    {
        public int? EmployeeId { get; set; } // Member ID
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string AccessType { get; set; } = "User"; // Administrator or User
    }
}

