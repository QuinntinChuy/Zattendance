using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChurchAttendance.Data;
using ChurchAttendance.Models;

namespace ChurchAttendance.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Group)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                schedule.CreatedAt = DateTime.Now;
                _context.Schedules.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View(schedule);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}




