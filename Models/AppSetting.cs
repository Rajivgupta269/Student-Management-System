using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Models;

/// <summary>Single-row table holding application preferences that are really used by the app.</summary>
public class AppSetting
{
    public int Id { get; set; }

    [Required, StringLength(100), Display(Name = "Institution name")]
    public string InstitutionName { get; set; } = "ScholarHub College";

    [Range(5, 50, ErrorMessage = "Choose between 5 and 50.")]
    [Display(Name = "Students per page")]
    public int StudentsPerPage { get; set; } = 8;

    [Range(0, 100, ErrorMessage = "Enter a percentage from 0 to 100.")]
    [Display(Name = "Minimum attendance (%)")]
    public int MinAttendancePercent { get; set; } = 75;
}
