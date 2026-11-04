using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            TotalStudents = await _db.Students.CountAsync(),
            ActiveStudents = await _db.Students.CountAsync(s => s.IsActive),
            TotalCourses = await _db.Courses.CountAsync(),
            TotalFaculty = await _db.Faculty.CountAsync(),
            TotalDepartments = await _db.Departments.CountAsync(),
            RecentStudents = await _db.Students.Include(s => s.Course)
                .OrderByDescending(s => s.CreatedAt).Take(5).ToListAsync()
        };

        var perDept = await _db.Departments
            .Select(d => new { d.Name, Count = d.Students.Count })
            .OrderBy(d => d.Name).ToListAsync();
        vm.DeptLabels = perDept.Select(d => d.Name).ToList();
        vm.DeptCounts = perDept.Select(d => d.Count).ToList();

        // Attendance overview (all calculated from stored records)
        var att = await _db.AttendanceRecords.GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
        var extras = vm.Extras;
        extras.Present = att.FirstOrDefault(a => a.Status == "Present")?.Count ?? 0;
        extras.Late = att.FirstOrDefault(a => a.Status == "Late")?.Count ?? 0;
        extras.Absent = att.FirstOrDefault(a => a.Status == "Absent")?.Count ?? 0;
        var total = extras.Present + extras.Late + extras.Absent;
        extras.AverageAttendance = total == 0 ? null : Math.Round((extras.Present + extras.Late) * 100.0 / total, 1);

        var today = DateTime.Today;
        extras.UpcomingExams = await _db.Examinations.Include(e => e.Course)
            .Where(e => e.ExamDate >= today && e.Status == "Scheduled")
            .OrderBy(e => e.ExamDate).Take(5).AsNoTracking().ToListAsync();

        return View(vm);
    }
}
