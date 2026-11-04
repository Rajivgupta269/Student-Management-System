using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Models;

public class Examination
{
    public static readonly string[] Statuses = { "Scheduled", "Completed", "Cancelled" };

    public int Id { get; set; }

    [Required, StringLength(120), Display(Name = "Examination Name")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Examination Date")]
    public DateTime ExamDate { get; set; } = DateTime.Today.AddDays(7);

    [Range(1, 1000, ErrorMessage = "Maximum marks must be between 1 and 1000.")]
    [Display(Name = "Maximum Marks")]
    public int MaxMarks { get; set; } = 100;

    [Range(0, 1000, ErrorMessage = "Passing marks must be between 0 and 1000.")]
    [Display(Name = "Passing Marks")]
    public int PassingMarks { get; set; } = 40;

    [Range(1, 8, ErrorMessage = "Semester must be between 1 and 8.")]
    public int Semester { get; set; } = 1;

    [Required, RegularExpression("^(Scheduled|Completed|Cancelled)$")]
    public string Status { get; set; } = "Scheduled";

    public ICollection<Result> Results { get; set; } = new List<Result>();
}
