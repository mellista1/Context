using backend.Entities;
using backend.Entities.Business;
using backend.Entities.Order;
using backend.Entities.Products;
using backend.Entities.Calendar;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Business> Businesses => Set<Business>();

    public DbSet<BusinessRole> BusinessRoles => Set<BusinessRole>();

    public DbSet<BusinessMembership> BusinessMemberships => Set<BusinessMembership>();

    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        });

        builder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.TableNumber).IsRequired();

            entity.Property(o => o.CreatedAt).IsRequired();

            entity.Ignore(o => o.TotalPrice);

            entity.HasMany(o => o.Products)
                .WithMany()
                .UsingEntity(j => j.ToTable("OrderProducts"));
        });

        builder.Entity<Business>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(150);
            
            entity.Property(b => b.Description)
                .HasMaxLength(1000);

            entity.Property(b => b.Address)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(b => b.IsActive)
                .IsRequired();

            entity.Property(b => b.CreatedAt)
                .IsRequired();
        });

        builder.Entity<BusinessRole>(entity =>
        {
            entity.HasKey(br => br.Id);

            entity.Property(br => br.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(br => br.Description)
                .HasMaxLength(500);

            entity.HasIndex(br => br.Name)
                .IsUnique();
        });

        builder.Entity<BusinessMembership>(entity =>
        {
            entity.HasKey(bm => bm.Id);

            entity.Property(bm => bm.UserId)
                .IsRequired();

            entity.Property(bm => bm.BusinessId)
                .IsRequired();

            entity.Property(bm => bm.RoleId)
                .IsRequired();

            entity.Property(bm => bm.IsActive)
                .IsRequired();

            entity.Property(bm => bm.CreatedAt)
                .IsRequired();

            entity.HasOne(bm => bm.User)
                .WithMany(u => u.BusinessMemberships)
                .HasForeignKey(bm => bm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bm => bm.Business)
                .WithMany(b => b.Memberships)
                .HasForeignKey(bm => bm.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bm => bm.Role)
                .WithMany(r => r.Memberships)
                .HasForeignKey(bm => bm.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(bm => new { bm.UserId, bm.BusinessId })
                .IsUnique();
        });

        builder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(ce => ce.Id);

            entity.Property(ce => ce.EventType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(ce => ce.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(ce => ce.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(ce => ce.Location)
                .HasMaxLength(200);

            entity.Property(ce => ce.EventDate).IsRequired();
            entity.Property(ce => ce.CreatedAt).IsRequired();

            entity.HasOne(ce => ce.Business)
                .WithMany()
                .HasForeignKey(ce => ce.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate entries for the same event
            entity.HasIndex(ce => new { ce.BusinessId, ce.Title, ce.EventDate })
                .IsUnique();
        });
    }
}