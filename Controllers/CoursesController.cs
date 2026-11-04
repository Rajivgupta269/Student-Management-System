using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;

namespace ScholarHub.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _db;
    public CoursesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.Courses.Include(c => c.Department).Include(c => c.Students)
            .OrderBy(c => c.Code).AsNoTracking().ToListAsync());

    public async Task<IActionResult> Create()
    {
        await LoadLists();
        return View(new Course());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,Name,Description,DepartmentId,DurationYears,IsActive")] Course c)
    {
        c.Code = c.Code?.Trim().ToUpper() ?? "";
        if (await _db.Courses.AnyAsync(x => x.Code == c.Code))
            ModelState.AddModelError(nameof(Course.Code), "This course code already exists.");
        if (!ModelState.IsValid) { await LoadLists(c); return View(c); }
        _db.Courses.Add(c);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Course added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var c = await _db.Courses.FindAsync(id);
        if (c == null) return NotFound();
        await LoadLists(c);
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var c = await _db.Courses.FindAsync(id);
        if (c == null) return NotFound();
        var ok = await TryUpdateModelAsync(c, "", x => x.Code, x => x.Name, x => x.Description,
            x => x.DepartmentId, x => x.DurationYears, x => x.IsActive);
        c.Code = c.Code.Trim().ToUpper();
        if (await _db.Courses.AnyAsync(x => x.Code == c.Code && x.Id != c.Id))
            ModelState.AddModelError(nameof(Course.Code), "This course code already exists.");
        if (!ok || !ModelState.IsValid) { await LoadLists(c); return View(c); }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Course updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Courses.FindAsync(id);
        if (c == null) return RedirectToAction(nameof(Index));
        var inUse = await _db.Students.AnyAsync(s => s.CourseId == id)
                 || await _db.Examinations.AnyAsync(e => e.CourseId == id)
                 || await _db.AttendanceRecords.AnyAsync(a => a.CourseId == id);
        if (inUse)
        {
            TempData["Error"] = "Cannot delete a course that has students, examinations or attendance records.";
            return RedirectToAction(nameof(Index));
        }
        _db.Courses.Remove(c);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Course deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLists(Course? c = null) =>
        ViewBag.Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", c?.DepartmentId);
}
