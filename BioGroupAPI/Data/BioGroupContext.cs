using BioGroupAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BioGroupAPI.Data;

public class BioGroupContext : DbContext
{
    public BioGroupContext(DbContextOptions<BioGroupContext> options)
        : base(options)
    {
    }

    // ================= DB SET =================
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrderTracking> OrderTrackings => Set<OrderTracking>();
    public DbSet<DistributorProfile> DistributorProfiles => Set<DistributorProfile>();
    public DbSet<DistributorProduct> DistributorProducts => Set<DistributorProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================= PRODUCT =================
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Badge).HasMaxLength(50);
            entity.Property(e => e.BadgeClass).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
        });

        // ================= PRODUCT IMAGE =================
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.AltText)
                .HasMaxLength(255);

            // Quan hệ 1-n
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ================= USER =================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
        });

        // ================= ORDER =================
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        // ================= ORDER ITEM =================
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            entity.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
        });

        // ================= ORDER TRACKING =================
        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);

            entity.HasOne(ot => ot.Order)
                .WithMany(o => o.OrderTrackings)
                .HasForeignKey(ot => ot.OrderId);
        });

        // ================= DISTRIBUTOR PROFILE =================
        modelBuilder.Entity<DistributorProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BusinessName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TaxCode).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ================= DISTRIBUTOR PRODUCT =================
        modelBuilder.Entity<DistributorProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasOne(e => e.Distributor)
                  .WithMany(d => d.Products)
                  .HasForeignKey(e => e.DistributorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ================= PAYMENT =================
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId);
        });

        // ================= SEED DATA =================
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // ===== PRODUCTS =====
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Bio Wraps 10cm x 10cm",
                Category = "Bio Wraps",
                Icon = "🍊",
                Description = "Bao bì sinh học từ vỏ cam",
                Price = 50000,
                IsFeatured = true
            },
            new Product
            {
                Id = 2,
                Name = "Túi Rác Sinh Học 10L",
                Category = "Túi Rác",
                Icon = "🗑️",
                Description = "Túi rác phân hủy sinh học",
                Price = 75000
            }
        );

        // ===== PRODUCT IMAGES =====
        modelBuilder.Entity<ProductImage>().HasData(
            new ProductImage
            {
                Id = 1,
                ProductId = 1,
                ImagePath = "images/bio-wraps.jpg",
                AltText = "Bio Wraps",
                DisplayOrder = 1,
                IsMainImage = true
            },
            new ProductImage
            {
                Id = 2,
                ProductId = 2,
                ImagePath = "images/bio-bag.jpg",
                AltText = "Túi rác sinh học",
                DisplayOrder = 1,
                IsMainImage = true
            }
        );

        // ===== USER DEMO =====
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "demo@bio.com",
                PasswordHash = "123456",
                Name = "Demo User",
                IsActive = true
            }
        );
    }
}
