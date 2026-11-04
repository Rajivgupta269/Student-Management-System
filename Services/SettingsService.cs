using Microsoft.EntityFrameworkCore;
using ScholarHub.Data;
using ScholarHub.Models;

namespace ScholarHub.Services;

public class SettingsService
{
    private readonly ApplicationDbContext _db;
    public SettingsService(ApplicationDbContext db) => _db = db;

    public async Task<AppSetting> GetAsync() =>
        await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync() ?? new AppSetting();
}
