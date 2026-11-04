using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers;

public class ExaminationsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ExaminationsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(bool upcoming = false, int? courseId = null)
    {
        var query = _db.Examinations.Include(e => e.Course).Include(e => e.Results).AsQueryable();
        if (upcoming)
        {
            var today = DateTime.Today;
            query = query.Where(e => e.ExamDate >= today && e.Status == "Scheduled").OrderBy(e => e.ExamDate);
        }
        else query = query.OrderByDescending(e => e.ExamDate);
        if (courseId.HasValue) query = query.Where(e => e.CourseId == courseId);

        return View(new ExamListViewModel
        {
            Exams = await query.AsNoTracking().ToListAsync(),
            Upcoming = upcoming, CourseId = courseId,
            Courses = new SelectList(await _db.Courses.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", courseId)
        });
    }

    public async Task<IActionResult> Create()
    {
        await LoadLists();
        return View(new Examination());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,CourseId,ExamDate,MaxMarks,PassingMarks,Semester,Status")] Examination exam)
    {
        await ValidateExam(exam);
        if (!ModelState.IsValid) { await LoadLists(exam); return View(exam); }
        _db.Examinations.Add(exam);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Examination created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var exam = await _db.Examinations.FindAsync(id);
        if (exam == null) return NotFound();
        await LoadLists(exam);
        return View(exam);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var exam = await _db.Examinations.FindAsync(id);
        if (exam == null) return NotFound();
        var ok = await TryUpdateModelAsync(exam, "", e => e.Name, e => e.CourseId, e => e.ExamDate,
            e => e.MaxMarks, e => e.PassingMarks, e => e.Semester, e => e.Status);
        await ValidateExam(exam);
        if (!ok || !ModelState.IsValid) { await LoadLists(exam); return View(exam); }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Examination updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var exam = await _db.Examinations.FindAsync(id);
        if (exam == null) return RedirectToAction(nameof(Index));
        if (await _db.Results.AnyAsync(r => r.ExaminationId == id))
        {
            TempData["Error"] = "Cannot delete an examination that already has results.";
            return RedirectToAction(nameof(Index));
        }
        _db.Examinations.Remove(exam);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Examination deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateExam(Examination e)
    {
        if (e.PassingMarks > e.MaxMarks)
            ModelState.AddModelError(nameof(Examination.PassingMarks), "Passing marks cannot be more than maximum marks.");

        var course = await _db.Courses.FindAsync(e.CourseId);
        if (course == null) ModelState.AddModelError(nameof(Examination.CourseId), "Selected course does not exist.");
        else if (e.Semester > course.DurationYears * 2)
            ModelState.AddModelError(nameof(Examination.Semester), $"This course has only {course.DurationYears * 2} semesters.");

        // Never allow the maximum to drop below marks that are already saved
        if (e.Id > 0 && e.MaxMarks > 0 && await _db.Results.AnyAsync(r => r.ExaminationId == e.Id && r.MarksObtained > e.MaxMarks))
            ModelState.AddModelError(nameof(Examination.MaxMarks), "Some saved results are higher than this maximum. Correct those results first.");
    }

    private async Task LoadLists(Examination? e = null) =>
        ViewBag.Courses = new SelectList(await _db.Courses.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", e?.CourseId);
}
