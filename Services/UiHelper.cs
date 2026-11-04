namespace ScholarHub;

public static class UiHelper
{
    public static string StatusBadge(string status) => status switch
    {
        "Present" or "Completed" or "Pass" or "Active" => "badge-success",
        "Late" or "Scheduled" => "badge-warning",
        "Absent" or "Fail" => "badge-error",
        _ => "badge-muted"
    };
}
