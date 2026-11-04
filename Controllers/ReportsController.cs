using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;
using ScholarHub.Services;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly SettingsService _settings;

    public ReportsController(ApplicationDbContext db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public IActionResult Index() => View();

    // ---- 1. Student enrollment report (with CSV export) ----
    public async Task<IActionResult> Enrollment(int? departmentId, int? courseId, int? semester, string? status, string? export)
    {
        var query = _db.Students.Include(s => s.Department).Include(s => s.Course).AsQueryable();
        if (departmentId.HasValue) query = query.Where(s => s.DepartmentId == departmentId);
        if (courseId.HasValue) query = query.Where(s => s.CourseId == courseId);
        if (semester.HasValue) query = query.Where(s => s.Semester == semester);
        if (status == "active") query = query.Where(s => s.IsActive);
        else if (status == "inactive") query = query.Where(s => !s.IsActive);
        var students = await query.OrderBy(s => s.Department!.Name).ThenBy(s => s.FullName).AsNoTracking().ToListAsync();

        if (export == "csv")
        {
            var bytes = CsvWriter.Build(
                new[] { "Student ID", "Name", "Email", "Phone", "Department", "Course", "Semester", "Status", "Admission Date" },
                students.Select(s => new object?[] { s.StudentCode, s.FullName, s.Email, s.Phone, s.Department?.Name,
                    s.Course?.Name, s.Semester, s.IsActive ? "Active" : "Inactive", s.AdmissionDate.ToString("yyyy-MM-dd") }));
            return File(bytes, "text/csv", "enrollment-report.csv");
        }

        return View(new EnrollmentReportViewModel
        {
            Students = students, DepartmentId = departmentId, CourseId = courseId, Semester = semester, Status = status,
            Departments = new SelectList(await _db.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", departmentId),
            Courses = new SelectList(await _db.Courses.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", courseId)
        });
    }

    // ---- 2. Department-wise and course-wise student counts ----
    public async Task<IActionResult> Counts()
    {
        return View(new CountsReportViewModel
        {
            ByDepartment = await _db.Departments.OrderBy(d => d.Name)
                .Select(d => new CountRow { Label = d.Name, Code = d.Code, Total = d.Students.Count, Active = d.Students.Count(s => s.IsActive) })
                .ToListAsync(),
            ByCourse = await _db.Courses.OrderBy(c => c.Name)
                .Select(c => new CountRow { Label = c.Name, Code = c.Code, Total = c.Students.Count, Active = c.Students.Count(s => s.IsActive) })
                .ToListAsync()
        });
    }

    // ---- 3. Attendance report: student-wise percentages for a course ----
    public async Task<IActionResult> Attendance(int? courseId, DateTime? from, DateTime? to)
    {
        var vm = new AttendanceReportViewModel
        {
            CourseId = courseId, From = from, To = to,
            MinPercent = (await _settings.GetAsync()).MinAttendancePercent,
            Courses = new SelectList(await _db.Courses.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", courseId)
        };

        if (courseId.HasValue)
        {
            var records = _db.AttendanceRecords.Where(a => a.CourseId == courseId);
            if (from.HasValue) records = records.Where(a => a.Date >= from.Value.Date);
            if (to.HasValue) records = records.Where(a => a.Date <= to.Value.Date);

            var grouped = await records.GroupBy(a => a.StudentId).Select(g => new
            {
                StudentId = g.Key,
                Present = g.Count(x => x.Status == "Present"),
                Late = g.Count(x => x.Status == "Late"),
                Absent = g.Count(x => x.Status == "Absent")
            }).ToListAsync();

            var students = await _db.Students.Where(s => s.CourseId == courseId).OrderBy(s => s.FullName).AsNoTracking().ToListAsync();
            vm.Rows = students.Select(s =>
            {
                var g = grouped.FirstOrDefault(x => x.StudentId == s.Id);
                return new AttendanceSummaryRow
                {
                    Code = s.StudentCode, Name = s.FullName,
                    Present = g?.Present ?? 0, Late = g?.Late ?? 0, Absent = g?.Absent ?? 0
                };
            }).ToList();
        }
        return View(vm);
    }

    // ---- 4. Examination results report (with CSV export) ----
    public async Task<IActionResult> Results(int? examId, string? export)
    {
        var query = _db.Results.Include(r => r.Student).Include(r => r.Examination).AsQueryable();
        if (examId.HasValue) query = query.Where(r => r.ExaminationId == examId);
        var results = await query.OrderBy(r => r.Examination!.Name).ThenByDescending(r => r.MarksObtained).AsNoTracking().ToListAsync();

        if (export == "csv")
        {
            var bytes = CsvWriter.Build(
                new[] { "Student ID", "Name", "Examination", "Marks", "Maximum", "Percentage", "Grade", "Result" },
                results.Select(r => new object?[] { r.Student?.StudentCode, r.Student?.FullName, r.Examination?.Name,
                    r.MarksObtained, r.Examination?.MaxMarks, r.Percentage, r.Grade, r.Passed ? "Pass" : "Fail" }));
            return File(bytes, "text/csv", "results-report.csv");
        }

        var exams = await _db.Examinations.Include(e => e.Course).OrderByDescending(e => e.ExamDate).ToListAsync();
        return View(new ResultsReportViewModel
        {
            ExamId = examId, Results = results,
            Exams = new SelectList(exams.Select(e => new { e.Id, Text = $"{e.Name} ({e.Course?.Code})" }), "Id", "Text", examId)
        });
    }
}
