using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Models;

public class Faculty
{
    public int Id { get; set; }

    [Required, StringLength(20), Display(Name = "Faculty ID")]
    public string FacultyCode { get; set; } = string.Empty;

    [Required, StringLength(100), Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    public string Phone { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Required, StringLength(60)]
    public string Designation { get; set; } = string.Empty;

    [Required, DataType(DataType.Date), Display(Name = "Joining Date")]
    public DateTime JoiningDate { get; set; } = DateTime.Today;

    public bool IsActive { get; set; } = true;
}
