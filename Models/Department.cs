using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Models;

public class Department
{
    public int Id { get; set; }

    [Required, StringLength(10), Display(Name = "Department Code")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100), Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Faculty> Faculty { get; set; } = new List<Faculty>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
