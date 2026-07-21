using Microsoft.EntityFrameworkCore;
using CourseManagementApi.Data.Entities;

namespace CourseManagementApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<CourseTerm> CourseTerms { get; set; } = null!;
        public DbSet<CourseTermTemplate> CourseTermTemplates { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; } = null!;
        public DbSet<CandidateRegistration> CandidateRegistrations { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<AppUser> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();

            modelBuilder.Entity<CourseTerm>()
                .HasOne(t => t.Course)
                .WithMany(c => c.CourseTerms)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseTermTemplate>()
                .HasOne(t => t.Course)
                .WithMany(c => c.CourseTermTemplates)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Видалення терміну не видаляє студентів — просто обнуляє прив'язку.
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.CourseTerm)
                .WithMany(t => t.Enrollments)
                .HasForeignKey(e => e.CourseTermId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CandidateRegistration>()
                .HasIndex(c => c.PhoneE164);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
