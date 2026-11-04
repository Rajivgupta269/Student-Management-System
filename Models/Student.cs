using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Models;

public class Student
{
    public int Id { get; set; }

    [Required, StringLength(20), Display(Name = "Student ID")]
    public string StudentCode { get; set; } = string.Empty;

    [Required, StringLength(100), Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date), Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

    [Required]
    public string Gender { get; set; } = "Male";

    [Required, EmailAddress, StringLength(120), Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Address { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Range(1, 8, ErrorMessage = "Semester must be between 1 and 8.")]
    public int Semester { get; set; } = 1;

    [Required, DataType(DataType.Date), Display(Name = "Admission Date")]
    public DateTime AdmissionDate { get; set; } = DateTime.Today;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<Result> Results { get; set; } = new List<Result>();
}
