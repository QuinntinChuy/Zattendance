using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ChurchAttendance.Data;
using ChurchAttendance.Models;
using ChurchAttendance.ViewModels;

namespace ChurchAttendance.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var members = await _context.Members
                .Include(m => m.Group)
                .Where(m => m.IsActive)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();

            return View(members);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> BulkImport()
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(string memberData, int? defaultGroupId)
        {
            if (string.IsNullOrWhiteSpace(memberData))
            {
                ModelState.AddModelError("", "Please provide member data.");
                ViewBag.Groups = await _context.Groups.ToListAsync();
                return View();
            }

            var lines = memberData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var addedCount = 0;
            var skippedCount = 0;
            var groups = await _context.Groups.ToListAsync();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                // Parse tab-separated values: [Member ID] [Tab] [Name] [Tab] [Gender]
                var parts = trimmedLine.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length < 3) continue;

                var memberId = parts[0].Trim();
                var fullName = parts[1].Trim();
                var genderStr = parts[2].Trim();

                // Skip if data is redacted or missing
                if (memberId.Contains("*****") || fullName.Contains("*****") || string.IsNullOrWhiteSpace(memberId))
                {
                    // Handle special cases like "Jun[이준하]" or "Amy(서윤지)" in the ID column
                    if (memberId.Contains("[") || memberId.Contains("("))
                    {
                        // This is actually a name, shift columns
                        fullName = memberId;
                        genderStr = parts.Length > 1 ? parts[1].Trim() : "";
                        memberId = null;
                    }
                    else
                    {
                        skippedCount++;
                        continue;
                    }
                }

                // Parse gender
                Gender gender;
                if (genderStr.Equals("Male", StringComparison.OrdinalIgnoreCase))
                    gender = Gender.Male;
                else if (genderStr.Equals("Female", StringComparison.OrdinalIgnoreCase))
                    gender = Gender.Female;
                else
                {
                    skippedCount++;
                    continue;
                }

                // Parse name (handle Korean names in brackets)
                string cleanName = fullName;
                if (fullName.Contains("[") && fullName.Contains("]"))
                {
                    var bracketIndex = fullName.IndexOf('[');
                    cleanName = fullName.Substring(0, bracketIndex).Trim();
                }
                else if (fullName.Contains("(") && fullName.Contains(")"))
                {
                    var parenIndex = fullName.IndexOf('(');
                    cleanName = fullName.Substring(0, parenIndex).Trim();
                }

                var nameSplit = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameSplit.Length < 2)
                {
                    skippedCount++;
                    continue;
                }

                var firstName = string.Join(" ", nameSplit.Take(nameSplit.Length - 1));
                var lastName = nameSplit[nameSplit.Length - 1];

                // Check if member already exists by MemberNumber or Name
                var existingMember = await _context.Members
                    .FirstOrDefaultAsync(m => 
                        (!string.IsNullOrEmpty(memberId) && m.MemberNumber == memberId) || 
                        (m.FirstName == firstName && m.LastName == lastName));

                if (existingMember != null)
                {
                    skippedCount++;
                    continue;
                }

                // Find appropriate group based on gender
                var group = defaultGroupId.HasValue 
                    ? groups.FirstOrDefault(g => g.Id == defaultGroupId.Value && g.GenderRestriction == gender)
                    : groups.FirstOrDefault(g => g.GenderRestriction == gender && g.Type == GroupType.Adult);

                // Fallback to any group matching gender
                if (group == null)
                {
                    group = groups.FirstOrDefault(g => g.GenderRestriction == gender);
                }

                if (group == null)
                {
                    skippedCount++;
                    continue;
                }

                var member = new Member
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = gender,
                    GroupId = group.Id,
                    MemberNumber = string.IsNullOrEmpty(memberId) ? null : memberId,
                    DateJoined = DateTime.Now,
                    IsActive = true
                };

                _context.Members.Add(member);
                addedCount++;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully imported {addedCount} members. {skippedCount} rows were skipped.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Username already exists.");
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                // Parse role
                if (!Enum.TryParse<UserRole>(model.Role, true, out var userRole))
                {
                    ModelState.AddModelError("Role", "Invalid role selection.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Role = userRole,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Add user to role
                    var roleName = userRole == UserRole.Administrator ? "Administrator" : "User";
                    await _userManager.AddToRoleAsync(user, roleName);

                    TempData["SuccessMessage"] = $"User '{model.UserName}' has been successfully registered as {roleName}.";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateUserRegistration()
        {
            // Get all active members for the employee dropdown
            var members = await _context.Members
                .Where(m => m.IsActive)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();
            
            ViewBag.Members = members;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserRegistration(CreateUserRegistrationViewModel model)
        {
            // Validate password match
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            }

            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Username already exists.");
                    var members = await _context.Members
                        .Where(m => m.IsActive)
                        .OrderBy(m => m.LastName)
                        .ThenBy(m => m.FirstName)
                        .ToListAsync();
                    ViewBag.Members = members;
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    var members = await _context.Members
                        .Where(m => m.IsActive)
                        .OrderBy(m => m.LastName)
                        .ThenBy(m => m.FirstName)
                        .ToListAsync();
                    ViewBag.Members = members;
                    return View(model);
                }

                // Get member details if employee is selected
                Member? member = null;
                if (model.EmployeeId.HasValue)
                {
                    member = await _context.Members.FindAsync(model.EmployeeId.Value);
                }

                // Parse access type (default to User if not specified)
                var accessType = string.IsNullOrEmpty(model.AccessType) ? "User" : model.AccessType;
                if (!Enum.TryParse<UserRole>(accessType, true, out var userRole))
                {
                    userRole = UserRole.User;
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = member != null ? member.FullName : model.UserName,
                    Role = userRole,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Add user to role
                    var roleName = userRole == UserRole.Administrator ? "Administrator" : "User";
                    await _userManager.AddToRoleAsync(user, roleName);

                    TempData["SuccessMessage"] = $"User '{model.UserName}' has been successfully registered as {roleName}.";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Reload members for dropdown
            var allMembers = await _context.Members
                .Where(m => m.IsActive)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();
            ViewBag.Members = allMembers;
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult RegistrationCreated()
        {
            return View();
        }
    }
}
