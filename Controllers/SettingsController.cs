using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers;

public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;

    public SettingsController(ApplicationDbContext db, UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn)
    {
        _db = db;
        _users = users;
        _signIn = signIn;
    }

    public async Task<IActionResult> Index() => View(await Build());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGeneral([Bind("InstitutionName,StudentsPerPage,MinAttendancePercent", Prefix = "General")] AppSetting general)
    {
        if (!ModelState.IsValid) return View("Index", await Build(general));

        var setting = await _db.AppSettings.FirstOrDefaultAsync();
        if (setting == null) { setting = new AppSetting(); _db.AppSettings.Add(setting); }
        setting.InstitutionName = general.InstitutionName.Trim();
        setting.StudentsPerPage = general.StudentsPerPage;
        setting.MinAttendancePercent = general.MinAttendancePercent;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View("Index", await Build());

        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var result = await _users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View("Index", await Build());
        }
        await _signIn.RefreshSignInAsync(user);
        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SettingsPageViewModel> Build(AppSetting? general = null)
    {
        var setting = general ?? await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync() ?? new AppSetting();
        var user = await _users.GetUserAsync(User);
        return new SettingsPageViewModel { General = setting, Email = user?.Email ?? "" };
    }
}
