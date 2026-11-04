using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Models;

namespace ScholarHub.Data;

/// <summary>Runs at startup. Each block only inserts when its table is empty, so data is never duplicated or reset.</summary>
public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext db, UserManager<IdentityUser> users, IConfiguration config)
    {
        if (!await db.Departments.AnyAsync())
        {
            var cs = new Department { Code = "CS", Name = "Computer Science", Description = "Demo department" };
            var com = new Department { Code = "COM", Name = "Commerce", Description = "Demo department" };
            db.Departments.AddRange(cs, com);
            await db.SaveChangesAsync();

            db.Courses.AddRange(
                new Course { Code = "BCA", Name = "Bachelor of Computer Applications", DepartmentId = cs.Id, DurationYears = 3, Description = "Demo course" },
                new Course { Code = "BCOM", Name = "Bachelor of Commerce", DepartmentId = com.Id, DurationYears = 3, Description = "Demo course" });
            await db.SaveChangesAsync();
        }

        if (!await db.AppSettings.AnyAsync())
        {
            db.AppSettings.Add(new AppSetting());
            await db.SaveChangesAsync();
        }

        // First administrator account (only when there are no users at all).
        if (!await users.Users.AnyAsync())
        {
            var email = config["SeedAdmin:Email"];
            var password = config["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

            var admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Could not create admin user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
