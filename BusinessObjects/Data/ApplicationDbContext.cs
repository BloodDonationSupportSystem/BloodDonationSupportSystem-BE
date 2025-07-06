using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BloodGroup> BloodGroups { get; set; }
        public DbSet<BloodInventory> BloodInventories { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<ComponentType> ComponentTypes { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DonationEvent> DonationEvents { get; set; }
        public DbSet<DonorProfile> DonorProfiles { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationCapacity> LocationCapacities { get; set; }
        public DbSet<LocationStaffAssignment> LocationStaffAssignments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<DonorReminderSettings> DonorReminderSettings { get; set; }
        public DbSet<DonationAppointmentRequest> DonationAppointmentRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // RefreshToken relationships
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlogPost relationships
            modelBuilder.Entity<BlogPost>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // BloodInventory relationships
            modelBuilder.Entity<BloodInventory>()
                .HasOne(bi => bi.BloodGroup)
                .WithMany()
                .HasForeignKey(bi => bi.BloodGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BloodInventory>()
                .HasOne(bi => bi.ComponentType)
                .WithMany(ct => ct.BloodInventories)
                .HasForeignKey(bi => bi.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BloodInventory>()
                .HasOne(bi => bi.DonationEvent)
                .WithMany()
                .HasForeignKey(bi => bi.DonationEventId)
                .OnDelete(DeleteBehavior.Restrict);

            // BloodRequest relationships
            modelBuilder.Entity<BloodRequest>()
                .HasOne(br => br.User)
                .WithMany()
                .HasForeignKey(br => br.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BloodRequest>()
                .HasOne(br => br.BloodGroup)
                .WithMany()
                .HasForeignKey(br => br.BloodGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BloodRequest>()
                .HasOne(br => br.ComponentType)
                .WithMany()
                .HasForeignKey(br => br.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BloodRequest>()
                .HasOne(br => br.Location)
                .WithMany(l => l.BloodRequests)
                .HasForeignKey(br => br.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Document relationships
            modelBuilder.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // DonationEvent relationships
            modelBuilder.Entity<DonationEvent>()
                .HasOne(de => de.DonorProfile)
                .WithMany()
                .HasForeignKey(de => de.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationEvent>()
                .HasOne(de => de.BloodGroup)
                .WithMany()
                .HasForeignKey(de => de.BloodGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationEvent>()
                .HasOne(de => de.ComponentType)
                .WithMany(ct => ct.DonationEvents)
                .HasForeignKey(de => de.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationEvent>()
                .HasOne(de => de.Location)
                .WithMany(l => l.DonationEvents)
                .HasForeignKey(de => de.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix DonorProfile relationships - This was causing shadow property warnings
            modelBuilder.Entity<DonorProfile>()
                .HasOne(dp => dp.User)
                .WithOne()
                .HasForeignKey<DonorProfile>(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonorProfile>()
                .HasOne(dp => dp.BloodGroup)
                .WithMany()
                .HasForeignKey(dp => dp.BloodGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix Notification relationships - This was causing shadow property warnings
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // DonorReminderSettings relationships
            modelBuilder.Entity<DonorReminderSettings>()
                .HasOne(drs => drs.DonorProfile)
                .WithOne()
                .HasForeignKey<DonorReminderSettings>(drs => drs.DonorProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // DonationAppointmentRequest relationships
            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.Donor)
                .WithMany()
                .HasForeignKey(dar => dar.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.Location)
                .WithMany(l => l.AppointmentRequests)
                .HasForeignKey(dar => dar.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.BloodGroup)
                .WithMany()
                .HasForeignKey(dar => dar.BloodGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.ComponentType)
                .WithMany()
                .HasForeignKey(dar => dar.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.RelatedBloodRequest)
                .WithMany()
                .HasForeignKey(dar => dar.RelatedBloodRequestId)
                .OnDelete(DeleteBehavior.SetNull); // SetNull ?? không xóa appointment khi blood request b? xóa

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.InitiatedByUser)
                .WithMany()
                .HasForeignKey(dar => dar.InitiatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.ReviewedByUser)
                .WithMany()
                .HasForeignKey(dar => dar.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasOne(dar => dar.ConfirmedLocation)
                .WithMany(l => l.ConfirmedAppointmentRequests)
                .HasForeignKey(dar => dar.ConfirmedLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // LocationCapacity relationships
            modelBuilder.Entity<LocationCapacity>()
                .HasOne(lc => lc.Location)
                .WithMany(l => l.LocationCapacities)
                .HasForeignKey(lc => lc.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // LocationStaffAssignment relationships
            modelBuilder.Entity<LocationStaffAssignment>()
                .HasOne(lsa => lsa.Location)
                .WithMany(l => l.StaffAssignments)
                .HasForeignKey(lsa => lsa.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LocationStaffAssignment>()
                .HasOne(lsa => lsa.User)
                .WithMany()
                .HasForeignKey(lsa => lsa.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Set up appropriate indices for frequently queried fields
            modelBuilder.Entity<BloodInventory>()
                .HasIndex(bi => new { bi.BloodGroupId, bi.ComponentTypeId, bi.ExpirationDate, bi.Status });

            modelBuilder.Entity<BloodRequest>()
                .HasIndex(br => new { br.BloodGroupId, br.ComponentTypeId, br.Status, br.IsEmergency });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.RoleId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            // Add indices for DonorReminderSettings
            modelBuilder.Entity<DonorReminderSettings>()
                .HasIndex(drs => drs.DonorProfileId)
                .IsUnique();

            modelBuilder.Entity<DonorReminderSettings>()
                .HasIndex(drs => new { drs.EnableReminders, drs.LastReminderSentTime });

            // Add indices for DonationAppointmentRequest
            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasIndex(dar => new { dar.Status, dar.RequestType });

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasIndex(dar => dar.DonorId);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasIndex(dar => new { dar.LocationId, dar.PreferredDate });

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasIndex(dar => dar.RelatedBloodRequestId);

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasIndex(dar => new { dar.IsUrgent, dar.Priority });

            modelBuilder.Entity<DonationAppointmentRequest>()
                .HasIndex(dar => dar.ExpiresAt);

            // Add indices for Location management
            modelBuilder.Entity<LocationCapacity>()
                .HasIndex(lc => new { lc.LocationId, lc.TimeSlot, lc.DayOfWeek });

            modelBuilder.Entity<LocationCapacity>()
                .HasIndex(lc => new { lc.EffectiveDate, lc.ExpiryDate, lc.IsActive });

            modelBuilder.Entity<LocationStaffAssignment>()
                .HasIndex(lsa => new { lsa.LocationId, lsa.UserId, lsa.IsActive });

            modelBuilder.Entity<LocationStaffAssignment>()
                .HasIndex(lsa => new { lsa.UserId, lsa.IsActive });

            // Configure constraints and properties
            modelBuilder.Entity<BloodGroup>()
                .HasIndex(bg => bg.GroupName)
                .IsUnique();

            modelBuilder.Entity<ComponentType>()
                .HasIndex(ct => ct.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<Location>()
                .HasIndex(l => l.Name)
                .IsUnique();

            // LocationCapacity constraints
            modelBuilder.Entity<LocationCapacity>()
                .HasCheckConstraint("CK_LocationCapacity_TotalCapacity", "TotalCapacity >= 0");

            // LocationStaffAssignment unique constraint for active assignments
            modelBuilder.Entity<LocationStaffAssignment>()
                .HasIndex(lsa => new { lsa.LocationId, lsa.UserId })
                .HasFilter("IsActive = 1")
                .IsUnique();
        }
    }
}
