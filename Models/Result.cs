using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScholarHub.Models;

public class Result
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a student.")]
    [Display(Name = "Student")]
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select an examination.")]
    [Display(Name = "Examination")]
    public int ExaminationId { get; set; }
    public Examination? Examination { get; set; }

    [Range(0, 1000, ErrorMessage = "Marks must be between 0 and 1000.")]
    [Display(Name = "Marks Obtained")]
    public double MarksObtained { get; set; }

    [StringLength(200)]
    public string? Remarks { get; set; }

    // Calculated from the examination's criteria - never stored, so they can't become inconsistent.
    [NotMapped]
    public double Percentage => Examination is { MaxMarks: > 0 } e ? Math.Round(MarksObtained / e.MaxMarks * 100, 2) : 0;

    [NotMapped]
    public bool Passed => Examination != null && MarksObtained >= Examination.PassingMarks;

    [NotMapped]
    public string Grade => !Passed ? "F"
        : Percentage >= 90 ? "A+" : Percentage >= 80 ? "A" : Percentage >= 70 ? "B+"
        : Percentage >= 60 ? "B" : Percentage >= 50 ? "C" : "D";
}
