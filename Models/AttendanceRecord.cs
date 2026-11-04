using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Models;

public class AttendanceRecord
{
    public static readonly string[] Statuses = { "Present", "Absent", "Late" };

    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a student.")]
    [Display(Name = "Student")]
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required, RegularExpression("^(Present|Absent|Late)$", ErrorMessage = "Status must be Present, Absent or Late.")]
    public string Status { get; set; } = "Present";

    [StringLength(200)]
    public string? Remarks { get; set; }
}
