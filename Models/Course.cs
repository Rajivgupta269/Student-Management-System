using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(15), Display(Name = "Course Code")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(120), Display(Name = "Course Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Range(1, 6, ErrorMessage = "Duration must be 1 to 6 years.")]
    [Display(Name = "Duration (years)")]
    public int DurationYears { get; set; } = 3;

    public bool IsActive { get; set; } = true;

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Examination> Examinations { get; set; } = new List<Examination>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
