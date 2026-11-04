using Microsoft.AspNetCore.Mvc.Rendering;
using ScholarHub.Models;

namespace ScholarHub.ViewModels;

public class DashboardViewModel
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalFaculty { get; set; }
    public int TotalDepartments { get; set; }
    public List<Student> RecentStudents { get; set; } = new();
    public List<string> DeptLabels { get; set; } = new();
    public List<int> DeptCounts { get; set; } = new();
    public DashboardExtras Extras { get; set; } = new();
}

public class StudentListViewModel
{
    public PaginatedList<Student> Students { get; set; } = null!;
    public string? Q { get; set; }
    public int? DepartmentId { get; set; }
    public int? CourseId { get; set; }
    public int? Semester { get; set; }
    public string? Status { get; set; }
    public string? Sort { get; set; }
    public SelectList Departments { get; set; } = null!;
    public SelectList Courses { get; set; } = null!;
}
