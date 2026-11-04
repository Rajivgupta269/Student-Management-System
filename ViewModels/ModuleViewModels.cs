using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ScholarHub.Models;

namespace ScholarHub.ViewModels;

// ---------- Account / Settings ----------
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8), Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class SettingsPageViewModel
{
    public AppSetting General { get; set; } = new();
    public ChangePasswordViewModel Password { get; set; } = new();
    public string Email { get; set; } = string.Empty;
}

// ---------- Dashboard ----------
public class DashboardExtras
{
    public double? AverageAttendance { get; set; }
    public int Present { get; set; }
    public int Late { get; set; }
    public int Absent { get; set; }
    public List<Examination> UpcomingExams { get; set; } = new();
}

// ---------- Attendance ----------
public class AttendanceRow
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
}

public class MarkAttendanceViewModel
{
    public int CourseId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public List<AttendanceRow> Rows { get; set; } = new();
    public SelectList? Courses { get; set; }
}

public class AttendanceListViewModel
{
    public PaginatedList<AttendanceRecord> Records { get; set; } = null!;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? CourseId { get; set; }
    public string? Status { get; set; }
    public string? Q { get; set; }
    public SelectList Courses { get; set; } = null!;
}

// ---------- Examinations / Results ----------
public class ExamListViewModel
{
    public List<Examination> Exams { get; set; } = new();
    public bool Upcoming { get; set; }
    public int? CourseId { get; set; }
    public SelectList Courses { get; set; } = null!;
}

public class ResultListViewModel
{
    public PaginatedList<Result> Results { get; set; } = null!;
    public int? ExamId { get; set; }
    public string? Q { get; set; }
    public string? Outcome { get; set; }
    public SelectList Exams { get; set; } = null!;
}

// ---------- Reports ----------
public class EnrollmentReportViewModel
{
    public List<Student> Students { get; set; } = new();
    public int? DepartmentId { get; set; }
    public int? CourseId { get; set; }
    public int? Semester { get; set; }
    public string? Status { get; set; }
    public SelectList Departments { get; set; } = null!;
    public SelectList Courses { get; set; } = null!;
}

public class CountRow
{
    public string Label { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Active { get; set; }
}

public class CountsReportViewModel
{
    public List<CountRow> ByDepartment { get; set; } = new();
    public List<CountRow> ByCourse { get; set; } = new();
}

public class AttendanceSummaryRow
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Present { get; set; }
    public int Late { get; set; }
    public int Absent { get; set; }
    public int Total => Present + Late + Absent;
    // Late still counts as attended
    public double Percentage => Total == 0 ? 0 : Math.Round((Present + Late) * 100.0 / Total, 1);
}

public class AttendanceReportViewModel
{
    public int? CourseId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int MinPercent { get; set; }
    public SelectList Courses { get; set; } = null!;
    public List<AttendanceSummaryRow> Rows { get; set; } = new();
}

public class ResultsReportViewModel
{
    public int? ExamId { get; set; }
    public SelectList Exams { get; set; } = null!;
    public List<Result> Results { get; set; } = new();
    public int Passed => Results.Count(r => r.Passed);
    public int Failed => Results.Count - Passed;
    public double AveragePercent => Results.Count == 0 ? 0 : Math.Round(Results.Average(r => r.Percentage), 1);
    public double HighestPercent => Results.Count == 0 ? 0 : Results.Max(r => r.Percentage);
}
public class CourseOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
}