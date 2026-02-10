using Authorization.Entities;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menus.Configurations;

public class MenuPermissionConfiguration : IEntityTypeConfiguration<MenuPermission>
{
    public void Configure(EntityTypeBuilder<MenuPermission> builder)
    {
        builder.ToTable("MenuPermissions");

        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Id)
            .HasColumnType("CHAR(36)");

        builder.Property(mp => mp.MenuId)
            .HasColumnType("CHAR(36)");

        builder.Property(mp => mp.PermissionId)
            .HasColumnType("CHAR(36)");

        builder.Property(mp => mp.PermissionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(mp => mp.Menu)
            .WithMany(m => m.MenuPermissions)
            .HasForeignKey(mp => mp.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mp => mp.Permission)
            .WithMany()
            .HasForeignKey(mp => mp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: same menu + permission + type combination
        builder.HasIndex(mp => new { mp.MenuId, mp.PermissionType, mp.PermissionId })
            .IsUnique();
    }
}
