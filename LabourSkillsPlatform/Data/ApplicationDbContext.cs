using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Data
{
    /// <summary>
    /// EF Core Code-First DbContext (Chapter 3, 3.3 - ORM: Entity Framework Core Code First).
    /// Implements the normalized relational schema described in Chapter 3, 3.3 (Database):
    /// Users, Roles, Employers, Labourers, Skills, Categories, Jobs, Applications, Reviews, Notifications.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employer> Employers => Set<Employer>();
        public DbSet<Labourer> Labourers => Set<Labourer>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<LabourerSkill> LabourerSkills => Set<LabourerSkill>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobSkill> JobSkills => Set<JobSkill>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---------- Employer (1:1 with ApplicationUser) ----------
            builder.Entity<Employer>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employer)
                .HasForeignKey<Employer>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Labourer (1:1 with ApplicationUser) ----------
            builder.Entity<Labourer>()
                .HasOne(l => l.User)
                .WithOne(u => u.Labourer)
                .HasForeignKey<Labourer>(l => l.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Category -> Skill (1:N) ----------
            builder.Entity<Skill>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Skills)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- LabourerSkill (M:N join: Labourer <-> Skill) ----------
            builder.Entity<LabourerSkill>()
                .HasOne(ls => ls.Labourer)
                .WithMany(l => l.LabourerSkills)
                .HasForeignKey(ls => ls.LabourerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LabourerSkill>()
                .HasOne(ls => ls.Skill)
                .WithMany(s => s.LabourerSkills)
                .HasForeignKey(ls => ls.SkillId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LabourerSkill>()
                .HasIndex(ls => new { ls.LabourerId, ls.SkillId })
                .IsUnique();

            // ---------- Employer -> Job (1:N) ----------
            builder.Entity<Job>()
                .HasOne(j => j.Employer)
                .WithMany(e => e.Jobs)
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Job>()
                .HasOne(j => j.Category)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- JobSkill (M:N join: Job <-> Skill) ----------
            builder.Entity<JobSkill>()
                .HasOne(js => js.Job)
                .WithMany(j => j.JobSkills)
                .HasForeignKey(js => js.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JobSkill>()
                .HasOne(js => js.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(js => js.SkillId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobSkill>()
                .HasIndex(js => new { js.JobId, js.SkillId })
                .IsUnique();

            // ---------- JobApplication (Job 1:N, Labourer 1:N) ----------
            builder.Entity<JobApplication>()
                .HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JobApplication>()
                .HasOne(a => a.Labourer)
                .WithMany(l => l.Applications)
                .HasForeignKey(a => a.LabourerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobApplication>()
                .HasIndex(a => new { a.JobId, a.LabourerId })
                .IsUnique();

            // ---------- Review ----------
            builder.Entity<Review>()
                .HasOne(r => r.Job)
                .WithMany()
                .HasForeignKey(r => r.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany()
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Notification ----------
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
