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
        public DbSet<EmergencyRequest> EmergencyRequests { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RequestMatch> RequestMatches { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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
                .WithMany()
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
                .WithMany()
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

            // Fix EmergencyRequest relationships - This was causing shadow property warnings
            modelBuilder.Entity<EmergencyRequest>()
                .HasOne(er => er.BloodGroup)
                .WithMany()
                .HasForeignKey(er => er.BloodGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmergencyRequest>()
                .HasOne(er => er.ComponentType)
                .WithMany()
                .HasForeignKey(er => er.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix Notification relationships - This was causing shadow property warnings
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix RequestMatch relationships - This was causing shadow property warnings
            modelBuilder.Entity<RequestMatch>()
                .HasOne(rm => rm.BloodRequest)
                .WithMany()
                .HasForeignKey(rm => rm.RequestId)  // Use the correct FK property name
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestMatch>()
                .HasOne(rm => rm.EmergencyRequest)
                .WithMany()
                .HasForeignKey(rm => rm.EmergencyRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestMatch>()
                .HasOne(rm => rm.DonationEvent)
                .WithMany()
                .HasForeignKey(rm => rm.DonationEventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Set up appropriate indices for frequently queried fields
            modelBuilder.Entity<BloodInventory>()
                .HasIndex(bi => new { bi.BloodGroupId, bi.ComponentTypeId, bi.ExpirationDate, bi.Status });

            modelBuilder.Entity<BloodRequest>()
                .HasIndex(br => new { br.BloodGroupId, br.ComponentTypeId, br.Status });

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

            // Removed indices for EmailVerificationToken and PasswordResetToken
            // Since we're using in-memory TokenStorage instead of database columns

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
        }
    }
}
