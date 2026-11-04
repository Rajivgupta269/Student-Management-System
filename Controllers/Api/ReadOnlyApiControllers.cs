using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;

namespace ScholarHub.Controllers.Api;

[ApiController]
[Route("api/departments")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class DepartmentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DepartmentsApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _db.Departments.OrderBy(d => d.Name)
        .Select(d => new { d.Id, d.Code, d.Name, d.Description, Students = d.Students.Count }).ToListAsync());
}

[ApiController]
[Route("api/courses")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CoursesApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public CoursesApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _db.Courses.OrderBy(c => c.Name)
        .Select(c => new { c.Id, c.Code, c.Name, c.DepartmentId, c.DurationYears, c.IsActive, Students = c.Students.Count }).ToListAsync());
}

[ApiController]
[Route("api/faculty")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class FacultyApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public FacultyApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _db.Faculty.OrderBy(f => f.FullName)
        .Select(f => new { f.Id, f.FacultyCode, f.FullName, f.Email, f.DepartmentId, f.Designation, f.IsActive }).ToListAsync());
}

[ApiController]
[Route("api/results")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ResultsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ResultsApiController(ApplicationDbContext db) => _db = db;

    /// <summary>GET api/results/student/5</summary>
    [HttpGet("student/{studentId:int}")]
    public async Task<IActionResult> ForStudent(int studentId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId)) return NotFound();
        var results = await _db.Results.Include(r => r.Examination).Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.Examination!.ExamDate).AsNoTracking().ToListAsync();
        return Ok(results.Select(r => new
        {
            r.Id, Examination = r.Examination!.Name, r.Examination.ExamDate, r.MarksObtained,
            Maximum = r.Examination.MaxMarks, r.Percentage, r.Grade, Result = r.Passed ? "Pass" : "Fail"
        }));
    }
}

[ApiController]
[Route("api/stats")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class StatsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public StatsApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var present = await _db.AttendanceRecords.CountAsync(a => a.Status == "Present" || a.Status == "Late");
        var total = await _db.AttendanceRecords.CountAsync();
        return Ok(new
        {
            students = await _db.Students.CountAsync(),
            courses = await _db.Courses.CountAsync(),
            departments = await _db.Departments.CountAsync(),
            faculty = await _db.Faculty.CountAsync(),
            averageAttendancePercent = total == 0 ? (double?)null : Math.Round(present * 100.0 / total, 1)
        });
    }
}
