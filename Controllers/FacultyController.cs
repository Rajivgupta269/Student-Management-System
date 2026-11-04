using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;

namespace ScholarHub.Controllers;

public class FacultyController : Controller
{
    private readonly ApplicationDbContext _db;
    public FacultyController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Faculty.Include(f => f.Department).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(f => f.FullName.Contains(q) || f.FacultyCode.Contains(q));
        }
        ViewBag.Q = q;
        return View(await query.OrderBy(f => f.FacultyCode).AsNoTracking().ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadLists();
        return View(new Faculty());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FacultyCode,FullName,Email,Phone,DepartmentId,Designation,JoiningDate,IsActive")] Faculty f)
    {
        f.FacultyCode = f.FacultyCode?.Trim() ?? "";
        if (await _db.Faculty.AnyAsync(x => x.FacultyCode == f.FacultyCode))
            ModelState.AddModelError(nameof(Faculty.FacultyCode), "This Faculty ID already exists.");
        if (!ModelState.IsValid) { await LoadLists(f); return View(f); }
        _db.Faculty.Add(f);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Faculty member added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var f = await _db.Faculty.FindAsync(id);
        if (f == null) return NotFound();
        await LoadLists(f);
        return View(f);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var f = await _db.Faculty.FindAsync(id);
        if (f == null) return NotFound();
        var ok = await TryUpdateModelAsync(f, "", x => x.FacultyCode, x => x.FullName, x => x.Email,
            x => x.Phone, x => x.DepartmentId, x => x.Designation, x => x.JoiningDate, x => x.IsActive);
        if (await _db.Faculty.AnyAsync(x => x.FacultyCode == f.FacultyCode && x.Id != f.Id))
            ModelState.AddModelError(nameof(Faculty.FacultyCode), "This Faculty ID already exists.");
        if (!ok || !ModelState.IsValid) { await LoadLists(f); return View(f); }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Faculty member updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var f = await _db.Faculty.FindAsync(id);
        if (f != null)
        {
            _db.Faculty.Remove(f);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Faculty member deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLists(Faculty? f = null) =>
        ViewBag.Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", f?.DepartmentId);
}
