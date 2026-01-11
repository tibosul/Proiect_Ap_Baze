using Microsoft.EntityFrameworkCore;
using VolunteerSystem.Core.Entities;

namespace VolunteerSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback or throw exception if not configured. 
                // For security, do not hardcode secrets here.
                // ideally, the context should be configured via DI.
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure TPT (Table Per Type) mapping strategy
            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<Volunteer>().ToTable("VolunteerProfiles");
            modelBuilder.Entity<Organizer>().ToTable("OrganizerProfiles");
            modelBuilder.Entity<Admin>().ToTable("AdminProfiles");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            modelBuilder.Entity<Application>(entity =>
            {
                entity.Property(e => e.AppliedDate).HasColumnName("AppliedAt");
                entity.Ignore(e => e.IsPresent); // Not in SQL table, handled by EventAttendance
            });
        }
    }
}
