using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;

namespace ScholarHub.Services;

/// <summary>Business rules for students, shared by the MVC controller and the REST API.</summary>
public static class StudentRules
{
    public static async Task<List<(string Field, string Message)>> ValidateAsync(
        ApplicationDbContext db, int id, string code, DateTime dob, int departmentId, int courseId, int semester)
    {
        var errors = new List<(string Field, string Message)>();

        if (await db.Students.AnyAsync(x => x.StudentCode == code && x.Id != id))
            errors.Add(("StudentCode", "This Student ID already exists."));
        if (dob.Date >= DateTime.Today)
            errors.Add(("DateOfBirth", "Date of birth must be in the past."));
        if (!await db.Departments.AnyAsync(d => d.Id == departmentId))
            errors.Add(("DepartmentId", "Selected department does not exist."));

        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            errors.Add(("CourseId", "Selected course does not exist."));
        else
        {
            if (course.DepartmentId != departmentId)
                errors.Add(("CourseId", "Selected course does not belong to the selected department."));
            if (semester > course.DurationYears * 2)
                errors.Add(("Semester", $"This course has only {course.DurationYears * 2} semesters."));
        }
        return errors;
    }
}
