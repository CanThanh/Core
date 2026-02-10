using BuildingBlocks.Common.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BuildingBlocks.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from all assemblies
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply configurations from other modules
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.StartsWith("Identity") == true ||
                       a.FullName?.StartsWith("Authorization") == true ||
                       a.FullName?.StartsWith("Users") == true ||
                       a.FullName?.StartsWith("Assets") == true ||
                       a.FullName?.StartsWith("Inventory") == true ||
                       a.FullName?.StartsWith("Maintenance") == true ||
                       a.FullName?.StartsWith("Menus") == true);

        foreach (var assembly in assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
