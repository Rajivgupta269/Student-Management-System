using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers;

public class AttendanceController : Controller
{
    private const int PageSize = 15;
    private readonly ApplicationDbContext _db;
    public AttendanceController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int? courseId, string? status, string? q, int page = 1)
    {
        var query = _db.AttendanceRecords.Include(a => a.Student).Include(a => a.Course).AsQueryable();
        if (from.HasValue) query = query.Where(a => a.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(a => a.Date <= to.Value.Date);
        if (courseId.HasValue) query = query.Where(a => a.CourseId == courseId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(a => a.Student!.FullName.Contains(q) || a.Student.StudentCode.Contains(q));
        }
        query = query.OrderByDescending(a => a.Date).ThenBy(a => a.Student!.FullName);

        var vm = new AttendanceListViewModel
        {
            Records = await PaginatedList<AttendanceRecord>.CreateAsync(query.AsNoTracking(), page, PageSize),
            From = from, To = to, CourseId = courseId, Status = status, Q = q,
            Courses = await CourseList(courseId)
        };
        return View(vm);
    }

    // ---- Bulk entry: pick a course and a date, mark every student ----
    [HttpGet]
    public async Task<IActionResult> Mark(int? courseId, DateTime? date)
    {
        var vm = new MarkAttendanceViewModel { CourseId = courseId ?? 0, Date = (date ?? DateTime.Today).Date };
        if (vm.CourseId > 0) await FillRows(vm);
        vm.Courses = await CourseList(vm.CourseId);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(MarkAttendanceViewModel vm)
    {
        vm.Date = vm.Date.Date;
        if (vm.Date > DateTime.Today) ModelState.AddModelError(nameof(vm.Date), "Date cannot be in the future.");
        if (!await _db.Courses.AnyAsync(c => c.Id == vm.CourseId)) ModelState.AddModelError(nameof(vm.CourseId), "Please select a valid course.");

        // Only active students of this course may be marked
        var allowed = (await _db.Students.Where(s => s.CourseId == vm.CourseId && s.IsActive)
            .Select(s => s.Id).ToListAsync()).ToHashSet();
        if (vm.Rows.Any(r => !allowed.Contains(r.StudentId) || !AttendanceRecord.Statuses.Contains(r.Status)))
            ModelState.AddModelError("", "The attendance list contains an invalid student or status. Please reload the page.");

        if (!ModelState.IsValid)
        {
            vm.Courses = await CourseList(vm.CourseId);
            return View(vm);
        }

        var existing = await _db.AttendanceRecords
            .Where(a => a.CourseId == vm.CourseId && a.Date == vm.Date).ToListAsync();
        foreach (var row in vm.Rows)
        {
            var record = existing.FirstOrDefault(a => a.StudentId == row.StudentId);
            if (record == null)
            {
                _db.AttendanceRecords.Add(new AttendanceRecord
                {
                    StudentId = row.StudentId, CourseId = vm.CourseId, Date = vm.Date,
                    Status = row.Status, Remarks = row.Remarks
                });
            }
            else
            {
                record.Status = row.Status; // update instead of duplicating
                record.Remarks = row.Remarks;
            }
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Attendance saved for {vm.Rows.Count} student(s) on {vm.Date:dd MMM yyyy}.";
        return RedirectToAction(nameof(Mark), new { courseId = vm.CourseId, date = vm.Date.ToString("yyyy-MM-dd") });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var record = await _db.AttendanceRecords.Include(a => a.Student).Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);
        return record == null ? NotFound() : View(record);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var record = await _db.AttendanceRecords.Include(a => a.Student).Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);
        if (record == null) return NotFound();
        var ok = await TryUpdateModelAsync(record, "", a => a.Status, a => a.Remarks);
        if (!ok || !ModelState.IsValid) return View(record);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Attendance record updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _db.AttendanceRecords.FindAsync(id);
        if (record != null)
        {
            _db.AttendanceRecords.Remove(record);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Attendance record deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task FillRows(MarkAttendanceViewModel vm)
    {
        var students = await _db.Students.Where(s => s.CourseId == vm.CourseId && s.IsActive)
            .OrderBy(s => s.FullName).AsNoTracking().ToListAsync();
        var existing = await _db.AttendanceRecords.Where(a => a.CourseId == vm.CourseId && a.Date == vm.Date)
            .AsNoTracking().ToListAsync();
        vm.Rows = students.Select(s =>
        {
            var rec = existing.FirstOrDefault(a => a.StudentId == s.Id);
            return new AttendanceRow
            {
                StudentId = s.Id, StudentCode = s.StudentCode, StudentName = s.FullName,
                Status = rec?.Status ?? "Present", Remarks = rec?.Remarks
            };
        }).ToList();
    }

    private async Task<SelectList> CourseList(int? selected) =>
        new SelectList(await _db.Courses.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", selected);
}
