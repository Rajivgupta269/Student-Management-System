using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;
using ScholarHub.Services;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers;

public class StudentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly SettingsService _settings;

    public StudentsController(ApplicationDbContext db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<IActionResult> Index(string? q, int? departmentId, int? courseId,
        int? semester, string? status, string? sort, int page = 1)
    {
        var pageSize = (await _settings.GetAsync()).StudentsPerPage;
        var query = _db.Students.Include(s => s.Department).Include(s => s.Course).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(s => s.FullName.Contains(q) || s.StudentCode.Contains(q));
        }
        if (departmentId.HasValue) query = query.Where(s => s.DepartmentId == departmentId);
        if (courseId.HasValue) query = query.Where(s => s.CourseId == courseId);
        if (semester.HasValue) query = query.Where(s => s.Semester == semester);
        if (status == "active") query = query.Where(s => s.IsActive);
        else if (status == "inactive") query = query.Where(s => !s.IsActive);

        query = sort switch
        {
            "name_desc" => query.OrderByDescending(s => s.FullName),
            "code" => query.OrderBy(s => s.StudentCode),
            "code_desc" => query.OrderByDescending(s => s.StudentCode),
            "admission" => query.OrderBy(s => s.AdmissionDate),
            "admission_desc" => query.OrderByDescending(s => s.AdmissionDate),
            _ => query.OrderBy(s => s.StudentCode) 
        };

        var vm = new StudentListViewModel
        {
            Students = await PaginatedList<Student>.CreateAsync(query.AsNoTracking(), page, pageSize),
            Q = q, DepartmentId = departmentId, CourseId = courseId,
            Semester = semester, Status = status, Sort = sort,
            Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", departmentId),
            Courses = new SelectList(await _db.Courses.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", courseId)
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var student = await _db.Students.Include(s => s.Department).Include(s => s.Course)
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return student == null ? NotFound() : View(student);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLists();
        return View(new Student());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentCode,FullName,DateOfBirth,Gender,Email,Phone,Address,DepartmentId,CourseId,Semester,AdmissionDate,IsActive")] Student student)
    {
        await ValidateStudent(student);
        if (!ModelState.IsValid)
        {
            await LoadLists(student);
            return View(student);
        }
        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Student {student.FullName} was added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();
        await LoadLists(student);
        return View(student);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();

        // TryUpdateModelAsync only updates the listed fields (prevents over-posting).
        var ok = await TryUpdateModelAsync(student, "",
            s => s.StudentCode, s => s.FullName, s => s.DateOfBirth, s => s.Gender, s => s.Email,
            s => s.Phone, s => s.Address, s => s.DepartmentId, s => s.CourseId, s => s.Semester,
            s => s.AdmissionDate, s => s.IsActive);

        await ValidateStudent(student);
        if (!ok || !ModelState.IsValid)
        {
            await LoadLists(student);
            return View(student);
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Student updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student != null)
        {
            _db.Students.Remove(student); // attendance and results of this student are removed too (cascade)
            await _db.SaveChangesAsync();
            TempData["Success"] = "Student deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateStudent(Student s)
    {
        s.StudentCode = s.StudentCode?.Trim() ?? "";
        foreach (var (field, message) in await StudentRules.ValidateAsync(
                     _db, s.Id, s.StudentCode, s.DateOfBirth, s.DepartmentId, s.CourseId, s.Semester))
            ModelState.AddModelError(field, message);
    }

    private async Task LoadLists(Student? s = null)
{
    ViewBag.Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", s?.DepartmentId);
    ViewBag.Courses = new SelectList(await _db.Courses.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", s?.CourseId);

    // Used by the form to show only the courses of the chosen department
    ViewBag.CourseOptions = await _db.Courses.Where(c => c.IsActive).OrderBy(c => c.Name)
        .Select(c => new CourseOption { Id = c.Id, Name = c.Name, DepartmentId = c.DepartmentId })
        .ToListAsync();
}
}
