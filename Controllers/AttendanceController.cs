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

        public async Task<IActionResult> Index(int? scheduleId)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Group)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            if (scheduleId.HasValue)
            {
                var schedule = await _context.Schedules
                    .Include(s => s.Group)
                    .FirstOrDefaultAsync(s => s.Id == scheduleId.Value);

                if (schedule == null)
                {
                    return NotFound();
                }

                var members = await _context.Members
                    .Include(m => m.Group)
                    .Where(m => m.IsActive && (schedule.GroupId == null || m.GroupId == schedule.GroupId))
                    .OrderBy(m => m.LastName)
                    .ThenBy(m => m.FirstName)
                    .ToListAsync();

                var attendances = await _context.Attendances
                    .Where(a => a.ScheduleId == scheduleId.Value)
                    .ToListAsync();

                var viewModel = new AttendanceIndexViewModel
                {
                    Schedule = schedule,
                    Members = members,
                    Attendances = attendances,
                    AllSchedules = schedules
                };

                return View(viewModel);
            }

            ViewBag.Schedules = schedules;
            return View(new AttendanceIndexViewModel { AllSchedules = schedules });
        }

        [HttpPost]
        [Authorize(Roles = "TeamLeader,Administrator")]
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
        [Authorize(Roles = "TeamLeader,Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(AddMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var member = new Member
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    GroupId = model.GroupId,
                    DateJoined = DateTime.Now,
                    IsActive = true
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Attendance");
            }

            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "TeamLeader,Administrator")]
        public async Task<IActionResult> AddMember()
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View();
        }
    }
}

