using System.ComponentModel.DataAnnotations;
using ScholarHub.Models;

namespace ScholarHub.ViewModels;

public class TokenRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class StudentDto
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string StudentCode { get; set; } = string.Empty;
    [Required, StringLength(100)] public string FullName { get; set; } = string.Empty;
    [Required] public DateTime DateOfBirth { get; set; }
    [Required, RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female or Other.")]
    public string Gender { get; set; } = "Male";
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = string.Empty;
    [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    public string Phone { get; set; } = string.Empty;
    [StringLength(250)] public string? Address { get; set; }
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Range(1, int.MaxValue)] public int CourseId { get; set; }
    [Range(1, 8)] public int Semester { get; set; } = 1;
    [Required] public DateTime AdmissionDate { get; set; }
    public bool IsActive { get; set; } = true;

    public static StudentDto From(Student s) => new()
    {
        Id = s.Id, StudentCode = s.StudentCode, FullName = s.FullName, DateOfBirth = s.DateOfBirth,
        Gender = s.Gender, Email = s.Email, Phone = s.Phone, Address = s.Address,
        DepartmentId = s.DepartmentId, CourseId = s.CourseId, Semester = s.Semester,
        AdmissionDate = s.AdmissionDate, IsActive = s.IsActive
    };

    public void CopyTo(Student s)
    {
        s.StudentCode = StudentCode.Trim(); s.FullName = FullName; s.DateOfBirth = DateOfBirth.Date;
        s.Gender = Gender; s.Email = Email; s.Phone = Phone; s.Address = Address;
        s.DepartmentId = DepartmentId; s.CourseId = CourseId; s.Semester = Semester;
        s.AdmissionDate = AdmissionDate.Date; s.IsActive = IsActive;
    }
}
