using System.Reflection;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the store. Entity configurations live in
/// <c>Persistence/Configurations</c> and are applied by convention.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Painting> Paintings => Set<Painting>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<StoredImage> Images => Set<StoredImage>();

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusEvent> OrderStatusEvents => Set<OrderStatusEvent>();

    public DbSet<CustomRequest> CustomRequests => Set<CustomRequest>();
    public DbSet<CustomRequestImage> CustomRequestImages => Set<CustomRequestImage>();
    public DbSet<CustomRequestStatusEvent> CustomRequestStatusEvents => Set<CustomRequestStatusEvent>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
