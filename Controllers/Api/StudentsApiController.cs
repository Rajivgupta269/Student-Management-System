using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;
using ScholarHub.Services;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers.Api;

/// <summary>Protected with JWT bearer tokens only (no cookies, so no CSRF risk). Get a token from POST api/auth/token.</summary>
[ApiController]
[Route("api/students")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class StudentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public StudentsApiController(ApplicationDbContext db) => _db = db;

    /// <summary>GET api/students?q=ali&amp;page=1&amp;pageSize=20</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Students.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(s => s.FullName.Contains(q) || s.StudentCode.Contains(q));
        }
        var total = await query.CountAsync();
        var items = await query.OrderBy(s => s.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items = items.Select(StudentDto.From) });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDto>> Get(int id)
    {
        var s = await _db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return s == null ? NotFound() : StudentDto.From(s);
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(StudentDto dto)
    {
        if (!await Validate(dto, 0)) return ValidationProblem(ModelState);
        var student = new Student();
        dto.CopyTo(student);
        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = student.Id }, StudentDto.From(student));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, StudentDto dto)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();
        if (!await Validate(dto, id)) return ValidationProblem(ModelState);
        dto.CopyTo(student);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();
        _db.Students.Remove(student);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> Validate(StudentDto dto, int id)
    {
        foreach (var (field, message) in await StudentRules.ValidateAsync(
                     _db, id, dto.StudentCode.Trim(), dto.DateOfBirth, dto.DepartmentId, dto.CourseId, dto.Semester))
            ModelState.AddModelError(field, message);
        return ModelState.IsValid;
    }
}
