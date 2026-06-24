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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
