using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers;

public class ResultsController : Controller
{
    private const int PageSize = 15;
    private readonly ApplicationDbContext _db;
    public ResultsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? examId, string? q, string? outcome, int page = 1)
    {
        var query = _db.Results.Include(r => r.Student).Include(r => r.Examination).ThenInclude(e => e!.Course).AsQueryable();
        if (examId.HasValue) query = query.Where(r => r.ExaminationId == examId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(r => r.Student!.FullName.Contains(q) || r.Student.StudentCode.Contains(q));
        }
        if (outcome == "pass") query = query.Where(r => r.MarksObtained >= r.Examination!.PassingMarks);
        else if (outcome == "fail") query = query.Where(r => r.MarksObtained < r.Examination!.PassingMarks);
        query = query.OrderByDescending(r => r.Examination!.ExamDate).ThenBy(r => r.Student!.FullName);

        return View(new ResultListViewModel
        {
            Results = await PaginatedList<Result>.CreateAsync(query.AsNoTracking(), page, PageSize),
            ExamId = examId, Q = q, Outcome = outcome,
            Exams = await ExamList(examId)
        });
    }

    public async Task<IActionResult> Create(int? examId)
    {
        var r = new Result { ExaminationId = examId ?? 0 };
        await LoadLists(r);
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentId,ExaminationId,MarksObtained,Remarks")] Result result)
    {
        await ValidateResult(result);
        if (!ModelState.IsValid) { await LoadLists(result); return View(result); }
        _db.Results.Add(result);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Result saved.";
        return RedirectToAction(nameof(Index), new { examId = result.ExaminationId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var r = await Load(id.Value);
        return r == null ? NotFound() : View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await Load(id);
        if (r == null) return NotFound();
        // Student and examination are fixed; only marks and remarks can change
        var ok = await TryUpdateModelAsync(r, "", x => x.MarksObtained, x => x.Remarks);
        await ValidateResult(r);
        if (!ok || !ModelState.IsValid) return View(r);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Result updated.";
        return RedirectToAction(nameof(Index), new { examId = r.ExaminationId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.Results.FindAsync(id);
        if (r != null)
        {
            _db.Results.Remove(r);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Result deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private Task<Result?> Load(int id) =>
        _db.Results.Include(r => r.Student).Include(r => r.Examination).ThenInclude(e => e!.Course)
            .FirstOrDefaultAsync(r => r.Id == id);

    private async Task ValidateResult(Result r)
    {
        var exam = await _db.Examinations.FindAsync(r.ExaminationId);
        var student = await _db.Students.FindAsync(r.StudentId);

        if (exam == null) ModelState.AddModelError(nameof(Result.ExaminationId), "Selected examination does not exist.");
        if (student == null) ModelState.AddModelError(nameof(Result.StudentId), "Selected student does not exist.");
        if (exam == null || student == null) return;

        if (r.MarksObtained > exam.MaxMarks)
            ModelState.AddModelError(nameof(Result.MarksObtained), $"Marks cannot be more than the maximum ({exam.MaxMarks}).");
        if (student.CourseId != exam.CourseId)
            ModelState.AddModelError(nameof(Result.StudentId), "This student is not enrolled in the examination's course.");
        if (await _db.Results.AnyAsync(x => x.StudentId == r.StudentId && x.ExaminationId == r.ExaminationId && x.Id != r.Id))
            ModelState.AddModelError(nameof(Result.StudentId), "A result for this student and examination already exists.");
    }

    private async Task LoadLists(Result? r = null)
    {
        var students = await _db.Students.Where(s => s.IsActive).OrderBy(s => s.FullName).ToListAsync();
        ViewBag.Students = new SelectList(students.Select(s => new { s.Id, Text = $"{s.StudentCode} - {s.FullName}" }), "Id", "Text", r?.StudentId);
        ViewBag.Exams = await ExamList(r?.ExaminationId);
    }

    private async Task<SelectList> ExamList(int? selected)
    {
        var exams = await _db.Examinations.Include(e => e.Course).OrderByDescending(e => e.ExamDate).ToListAsync();
        return new SelectList(exams.Select(e => new { e.Id, Text = $"{e.Name} ({e.Course?.Code}, max {e.MaxMarks})" }), "Id", "Text", selected);
    }
}
