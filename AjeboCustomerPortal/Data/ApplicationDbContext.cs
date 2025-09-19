using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AjeboCustomerPortal.Models;
using Microsoft.AspNetCore.Identity;

namespace AjeboCustomerPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserDetailes>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // --- Tables ---
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ===== Identity tables =====
            
            b.Entity<IdentityRole>().ToTable("AspNetRoles");
            b.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            b.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            b.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            b.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            b.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");

            // ===== Custom tables =====
            
            b.Entity<Apartment>().ToTable("Apartments");

            // Review relationships (prevent accidental cascade deletes)
            b.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Review>()
                .HasOne(r => r.Apartment)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.ApartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → User (explicit nav)
            b.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            b.Entity<Review>()
              .HasIndex(r => new { r.OrderId, r.ApartmentId, r.UserId })
              .IsUnique();

            // Cart → User (explicit nav)
            b.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cascades within aggregates (expected)
            b.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Money precision
            b.Entity<Apartment>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            b.Entity<Apartment>().Property(p => p.DefaultFare).HasColumnType("decimal(18,2)");
            b.Entity<OrderItem>().Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
            b.Entity<Order>().Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");

            // Geo precision
            b.Entity<Apartment>().Property(a => a.Latitude).HasColumnType("decimal(9,6)");
            b.Entity<Apartment>().Property(a => a.Longitude).HasColumnType("decimal(9,6)");

            // Helpful indexes
            b.Entity<Apartment>().HasIndex(a => a.City);
            b.Entity<Order>().HasIndex(o => new { o.UserId, o.CreatedAt });
            b.Entity<Review>().HasIndex(r => new { r.ApartmentId, r.CreatedAt });

            // Optional: One open cart per user (SQL Server filtered index)
            // b.Entity<Cart>()
            //   .HasIndex(c => new { c.UserId, c.IsCheckedOut })
            //   .HasFilter("[IsCheckedOut] = 0")
            //   .IsUnique();
        }

    }
}
