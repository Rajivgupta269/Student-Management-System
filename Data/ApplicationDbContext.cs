using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Models;

namespace ScholarHub.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Faculty> Faculty => Set<Faculty>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Examination> Examinations => Set<Examination>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b); // Identity tables

        b.Entity<Department>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Course>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Faculty>().HasIndex(x => x.FacultyCode).IsUnique();
        b.Entity<Student>().HasIndex(x => x.StudentCode).IsUnique();

        // Restrict: a parent cannot be deleted while children exist.
        b.Entity<Course>().HasOne(x => x.Department).WithMany(d => d.Courses)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Faculty>().HasOne(x => x.Department).WithMany(d => d.Faculty)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Student>().HasOne(x => x.Department).WithMany(d => d.Students)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Student>().HasOne(x => x.Course).WithMany(c => c.Students)
            .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);

        // Attendance: one record per student + course + day. Deleted together with the student.
        b.Entity<AttendanceRecord>().HasIndex(a => new { a.StudentId, a.CourseId, a.Date }).IsUnique();
        b.Entity<AttendanceRecord>().HasOne(a => a.Student).WithMany(s => s.AttendanceRecords)
            .HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<AttendanceRecord>().HasOne(a => a.Course).WithMany(c => c.AttendanceRecords)
            .HasForeignKey(a => a.CourseId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Examination>().HasOne(e => e.Course).WithMany(c => c.Examinations)
            .HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);

        // Results: one per student + exam. Deleted with the student; exam cannot be deleted while results exist.
        b.Entity<Result>().HasIndex(r => new { r.StudentId, r.ExaminationId }).IsUnique();
        b.Entity<Result>().HasOne(r => r.Student).WithMany(s => s.Results)
            .HasForeignKey(r => r.StudentId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Result>().HasOne(r => r.Examination).WithMany(e => e.Results)
            .HasForeignKey(r => r.ExaminationId).OnDelete(DeleteBehavior.Restrict);
    }
}
