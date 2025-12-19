using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChurchAttendance.Data;
using ChurchAttendance.Models;
using ChurchAttendance.ViewModels;

namespace ChurchAttendance.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? scheduleId, int? filterGroupId, string? filterPosition, string? tab = "Members")
        {
            var schedules = await _context.Schedules
                .Include(s => s.Group)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var allGroups = await _context.Groups.OrderBy(g => g.Name).ToListAsync();
            var allPositions = await _context.Members
                .Where(m => m.IsActive && !string.IsNullOrEmpty(m.Position))
                .Select(m => m.Position!)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            ViewBag.ActiveTab = tab ?? "Members";

            // For Members tab, always get all active members regardless of schedule
            if (tab == "Members")
            {
                var allMembers = await _context.Members
                    .Include(m => m.Group)
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.LastName)
                    .ThenBy(m => m.FirstName)
                    .ToListAsync();

                var membersViewModel = new AttendanceIndexViewModel 
                { 
                    AllSchedules = schedules,
                    AllGroups = allGroups,
                    AllPositions = allPositions,
                    Members = allMembers
                };
                
                return View(membersViewModel);
            }

            // For Position Holder tab, get all members with positions
            if (tab == "PositionHolder")
            {
                var positionHolders = await _context.Members
                    .Include(m => m.Group)
                    .Where(m => m.IsActive && !string.IsNullOrEmpty(m.Position))
                    .OrderBy(m => m.LastName)
                    .ThenBy(m => m.FirstName)
                    .ToListAsync();

                // Get all members for the dropdown (including those without positions)
                var allMembersForDropdown = await _context.Members
                    .Include(m => m.Group)
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.LastName)
                    .ThenBy(m => m.FirstName)
                    .ToListAsync();

                var positionHolderViewModel = new AttendanceIndexViewModel 
                { 
                    AllSchedules = schedules,
                    AllGroups = allGroups,
                    AllPositions = allPositions,
                    Members = positionHolders
                };
                
                ViewBag.AllMembersForPosition = allMembersForDropdown;
                return View(positionHolderViewModel);
            }

            if (scheduleId.HasValue)
            {
                var schedule = await _context.Schedules
                    .Include(s => s.Group)
                    .FirstOrDefaultAsync(s => s.Id == scheduleId.Value);

                if (schedule == null)
                {
                    return NotFound();
                }

                var membersQuery = _context.Members
                    .Include(m => m.Group)
                    .Where(m => m.IsActive && (schedule.GroupId == null || m.GroupId == schedule.GroupId));

                // Apply tab-specific filters
                if (tab == "PositionHolder")
                {
                    // Only show members with positions
                    membersQuery = membersQuery.Where(m => !string.IsNullOrEmpty(m.Position));
                }
                else if (tab == "GroupName")
                {
                    // Group by group name - we'll handle this in the view
                    // For now, just show all members
                }

                // Apply group filter
                if (filterGroupId.HasValue)
                {
                    membersQuery = membersQuery.Where(m => m.GroupId == filterGroupId.Value);
                }

                // Apply position filter
                if (!string.IsNullOrEmpty(filterPosition))
                {
                    membersQuery = membersQuery.Where(m => m.Position == filterPosition);
                }

                var members = await membersQuery
                    .OrderBy(m => m.LastName)
                    .ThenBy(m => m.FirstName)
                    .ToListAsync();

                var attendances = await _context.Attendances
                    .Where(a => a.ScheduleId == scheduleId.Value)
                    .ToListAsync();

                // For Group Name tab, get groups with member counts and leaders
                var groupsWithDetails = allGroups.Select(g => new
                {
                    Group = g,
                    TotalMembers = members.Count(m => m.GroupId == g.Id),
                    Leader = members.FirstOrDefault(m => m.GroupId == g.Id && m.IsActive && 
                        (m.Position != null && (m.Position.ToLower().Contains("leader") || m.Position.ToLower().Contains("head"))))
                }).ToList();

                var viewModel = new AttendanceIndexViewModel
                {
                    Schedule = schedule,
                    Members = members,
                    Attendances = attendances,
                    AllSchedules = schedules,
                    AllGroups = allGroups,
                    AllPositions = allPositions,
                    FilterGroupId = filterGroupId,
                    FilterPosition = filterPosition
                };

                ViewBag.GroupsWithDetails = groupsWithDetails;
                return View(viewModel);
            }

            // For Group Name tab, get all groups with member counts and leaders
            if (tab == "GroupName")
            {
                var allMembersForGroups = await _context.Members
                    .Include(m => m.Group)
                    .Where(m => m.IsActive)
                    .ToListAsync();

                var allGroupsWithDetails = allGroups.Select(g => new
                {
                    Group = g,
                    TotalMembers = allMembersForGroups.Count(m => m.GroupId == g.Id),
                    Leader = allMembersForGroups.FirstOrDefault(m => m.GroupId == g.Id && m.IsActive && 
                        (m.Position != null && (m.Position.ToLower().Contains("leader") || m.Position.ToLower().Contains("head"))))
                }).ToList();

                var groupNameViewModel = new AttendanceIndexViewModel 
                { 
                    AllSchedules = schedules,
                    AllGroups = allGroups,
                    AllPositions = allPositions
                };
                
                ViewBag.GroupsWithDetails = allGroupsWithDetails;
                ViewBag.AllMembersForGroup = allMembersForGroups;
                return View(groupNameViewModel);
            }

            // For Group Name tab when no schedule selected, get all groups
            var allMembersForGroupsDefault = await _context.Members
                .Include(m => m.Group)
                .Where(m => m.IsActive)
                .ToListAsync();

            var allGroupsWithDetailsDefault = allGroups.Select(g => new
            {
                Group = g,
                TotalMembers = allMembersForGroupsDefault.Count(m => m.GroupId == g.Id),
                Leader = allMembersForGroupsDefault.FirstOrDefault(m => m.GroupId == g.Id && m.IsActive && 
                    (m.Position != null && (m.Position.ToLower().Contains("leader") || m.Position.ToLower().Contains("head"))))
            }).ToList();

            var emptyViewModel = new AttendanceIndexViewModel 
            { 
                AllSchedules = schedules,
                AllGroups = allGroups,
                AllPositions = allPositions
            };
            
            ViewBag.GroupsWithDetails = allGroupsWithDetailsDefault;
            return View(emptyViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(int scheduleId, int memberId, AttendanceStatus status)
        {
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.ScheduleId == scheduleId && a.MemberId == memberId);

            if (existingAttendance != null)
            {
                existingAttendance.Status = status;
                existingAttendance.RecordedAt = DateTime.Now;
            }
            else
            {
                _context.Attendances.Add(new Attendance
                {
                    ScheduleId = scheduleId,
                    MemberId = memberId,
                    Status = status,
                    RecordedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { scheduleId });
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddMember([FromBody] AddMemberViewModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, errors = new[] { "Invalid request data." } });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return Json(new { success = false, errors = new[] { "Full Name is required." } });
            }

            // Parse full name into first and last name
            var nameParts = model.FullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 2)
            {
                return Json(new { success = false, errors = new[] { "Please provide both first and last name." } });
            }

            // Parse gender
            if (!Enum.TryParse<Gender>(model.Gender, true, out var gender))
            {
                return Json(new { success = false, errors = new[] { "Invalid gender selection." } });
            }

            // Validate group exists
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == model.GroupId);
            if (!groupExists)
            {
                return Json(new { success = false, errors = new[] { "Selected group does not exist." } });
            }

            var firstName = string.Join(" ", nameParts.Take(nameParts.Length - 1));
            var lastName = nameParts[nameParts.Length - 1];

            var member = new Member
            {
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                GroupId = model.GroupId,
                Position = model.Position,
                MemberNumber = model.LifeNumber,
                DateJoined = DateTime.Now,
                IsActive = true
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Member added successfully!" });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddGroupName([FromBody] AddGroupNameViewModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, errors = new[] { "Invalid request data." } });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.GroupName))
            {
                return Json(new { success = false, errors = new[] { "Group Name is required." } });
            }

            if (!model.LeaderId.HasValue)
            {
                return Json(new { success = false, errors = new[] { "Please select a leader." } });
            }

            // Check if group name already exists
            var existingGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Name == model.GroupName);
            if (existingGroup != null)
            {
                return Json(new { success = false, errors = new[] { "Group name already exists." } });
            }

            // Get the leader member
            var leader = await _context.Members.FindAsync(model.LeaderId.Value);
            if (leader == null)
            {
                return Json(new { success = false, errors = new[] { "Selected leader not found." } });
            }

            // Create new group
            var group = new Group
            {
                Name = model.GroupName,
                Type = GroupType.Adult, // Default type, can be changed later
                GenderRestriction = leader.Gender
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Assign selected members to the group
            if (model.SelectedMembers != null && model.SelectedMembers.Any())
            {
                var members = await _context.Members
                    .Where(m => model.SelectedMembers.Contains(m.Id))
                    .ToListAsync();

                foreach (var member in members)
                {
                    member.GroupId = group.Id;
                }

                // Also assign the leader to the group
                leader.GroupId = group.Id;
                await _context.SaveChangesAsync();
            }
            else
            {
                // If no members selected, at least assign the leader
                leader.GroupId = group.Id;
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true, message = "Group created successfully!" });
        }

        [HttpGet]
        [Authorize(Roles = "User,Administrator")]
        public async Task<IActionResult> AddMember()
        {
            ViewBag.Groups = await _context.Groups.OrderBy(g => g.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        [ValidateAntiForgeryToken]
        [ActionName("AddMember")]
        public async Task<IActionResult> AddMemberPost(AddMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Parse full name into first and last name
                var nameParts = model.FullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length < 2)
                {
                    ModelState.AddModelError("FullName", "Please provide both first and last name.");
                    ViewBag.Groups = await _context.Groups.OrderBy(g => g.Name).ToListAsync();
                    return View(model);
                }

                // Parse gender
                if (!Enum.TryParse<Gender>(model.Gender, true, out var gender))
                {
                    ModelState.AddModelError("Gender", "Invalid gender selection.");
                    ViewBag.Groups = await _context.Groups.OrderBy(g => g.Name).ToListAsync();
                    return View(model);
                }

                var firstName = string.Join(" ", nameParts.Take(nameParts.Length - 1));
                var lastName = nameParts[nameParts.Length - 1];

                var member = new Member
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = gender,
                    GroupId = model.GroupId,
                    Position = model.Position,
                    MemberNumber = model.LifeNumber,
                    DateJoined = DateTime.Now,
                    IsActive = true
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Attendance");
            }

            ViewBag.Groups = await _context.Groups.OrderBy(g => g.Name).ToListAsync();
            return View(model);
        }
    }
}

