using ETQAN.API.Models;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ETQAN.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        // ===== DbSets =====
        public DbSet<Client> Clients { get; set; }
        public DbSet<Artisan> Artisans { get; set; }
       //ah
       public DbSet<ArtisanPortfolio> ArtisanPortfolios { get; set; }
        //.
        public DbSet<Company> Companies { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
       // public DbSet<MaritalStatus> MaritalStatuses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== ApplicationUser: One-to-One Relationships =====
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Client)
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Artisan)
                .WithOne(a => a.User)
                .HasForeignKey<Artisan>(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<MaritalStatus>().HasData(
            //    new MaritalStatus { Id = 1, Name = "أعزب" },
            //    new MaritalStatus { Id = 2, Name = "متزوج" },
            //    new MaritalStatus { Id = 3, Name = "أرمل" },
            //    new MaritalStatus { Id = 4, Name = "مطلق" }
            
            modelBuilder.Entity<Job>().HasData(
                new Job { Id = 1, Name = "حداد" },
                new Job { Id = 2, Name = "كهرباء" },
                new Job { Id = 3, Name = "نجارة" },
                new Job { Id = 4, Name = "سباكة" },
                new Job { Id = 5, Name = "محارة" },
                new Job { Id = 6, Name = "نقاش" },
                new Job { Id = 7, Name = "فني غاز" },
                new Job { Id = 8, Name = "فني تكييفات" },
                new Job { Id = 9, Name = "منجد" },
                new Job { Id = 10, Name = "سيراميك" },
                new Job { Id = 11, Name = "أمن وأنظمة ذكية" },
                new Job { Id = 12, Name = "عامل بناء" },
                new Job { Id = 13, Name = "صيانة أجهزة كهربائية" },
                new Job { Id = 14, Name = "سواق نقل" },
                new Job { Id = 15, Name = "تكسير وإزالة" },
                new Job { Id = 16, Name = "الومنتال" },
                new Job { Id = 17, Name = "فني تركيب دش" },
                new Job { Id = 18, Name = "تنظيف" },
                new Job { Id = 19, Name = "استشارات هندسية" },
                new Job { Id = 20, Name = "رش مبيدات" }
           );

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithOne(c => c.User)
                .HasForeignKey<Company>(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            //ah
            modelBuilder.Entity<ArtisanPortfolio>()
                 .HasOne(p => p.Artisan)
                 .WithMany(a => a.Portfolio) 
                 .HasForeignKey(p => p.ArtisanId)
                 .OnDelete(DeleteBehavior.Cascade); 
            //.

            // ===== Reviews: Prevent Multiple Cascade Paths =====
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsWritten)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Review>()
            //    .HasOne(r => r.Artisan)
            //    .WithMany(a => a.Reviews)
            //    .HasForeignKey(r => r.ArtisanId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // ===== ServiceRequest Relationships =====
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(s => s.Client)
                .WithMany(c => c.Requests)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<ServiceRequest>()
            //    .HasOne(s => s.Artisan)
            //    .WithMany(a => a.ServiceRequests)
            //    .HasForeignKey(s => s.ArtisanId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // ===== Orders =====
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Remove ServiceRequests from ApplicationUser =====
            //// Only Client should have ServiceRequests navigation
            //modelBuilder.Entity<ApplicationUser>()
            //    .Ignore(u => u.ServiceRequests);

            // ===== Decimal Precision =====
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);
        }
    }
}