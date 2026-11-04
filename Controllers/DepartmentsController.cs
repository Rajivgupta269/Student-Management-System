using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;

namespace ScholarHub.Controllers;

public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    public DepartmentsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.Departments.Include(d => d.Students).Include(d => d.Courses).Include(d => d.Faculty)
           .OrderBy(d => d.Code).AsNoTracking().ToListAsync());

    public IActionResult Create() => View(new Department());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,Name,Description")] Department d)
    {
        d.Code = d.Code?.Trim().ToUpper() ?? "";
        if (await _db.Departments.AnyAsync(x => x.Code == d.Code))
            ModelState.AddModelError(nameof(Department.Code), "This department code already exists.");
        if (!ModelState.IsValid) return View(d);
        _db.Departments.Add(d);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Department added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var d = await _db.Departments.FindAsync(id);
        return d == null ? NotFound() : View(d);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var d = await _db.Departments.FindAsync(id);
        if (d == null) return NotFound();
        var ok = await TryUpdateModelAsync(d, "", x => x.Code, x => x.Name, x => x.Description);
        d.Code = d.Code.Trim().ToUpper();
        if (await _db.Departments.AnyAsync(x => x.Code == d.Code && x.Id != d.Id))
            ModelState.AddModelError(nameof(Department.Code), "This department code already exists.");
        if (!ok || !ModelState.IsValid) return View(d);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Department updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _db.Departments.Include(x => x.Students).Include(x => x.Courses).Include(x => x.Faculty)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return RedirectToAction(nameof(Index));
        if (d.Students.Any() || d.Courses.Any() || d.Faculty.Any())
        {
            TempData["Error"] = "Cannot delete a department that still has students, courses or faculty.";
            return RedirectToAction(nameof(Index));
        }
        _db.Departments.Remove(d);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Department deleted.";
        return RedirectToAction(nameof(Index));
    }
}
