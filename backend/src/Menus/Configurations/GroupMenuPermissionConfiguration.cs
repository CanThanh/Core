using Menus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menus.Configurations;

public class GroupMenuPermissionConfiguration : IEntityTypeConfiguration<GroupMenuPermission>
{
    public void Configure(EntityTypeBuilder<GroupMenuPermission> builder)
    {
        builder.ToTable("GroupMenuPermissions");

        builder.HasKey(gmp => gmp.Id);

        builder.Property(gmp => gmp.Id)
            .HasColumnType("CHAR(36)");

        builder.Property(gmp => gmp.GroupId)
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(gmp => gmp.MenuId)
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(gmp => gmp.PermissionId)
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(gmp => gmp.PermissionType)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("View, Create, Edit, Delete");

        builder.HasOne(gmp => gmp.Menu)
            .WithMany()
            .HasForeignKey(gmp => gmp.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(gmp => gmp.GroupId);
        builder.HasIndex(gmp => gmp.MenuId);
        builder.HasIndex(gmp => gmp.PermissionId);
        builder.HasIndex(gmp => gmp.PermissionType);

        // Unique constraint: Each group can have each permission type for a menu only once
        builder.HasIndex(gmp => new { gmp.GroupId, gmp.MenuId, gmp.PermissionId, gmp.PermissionType })
            .IsUnique()
            .HasDatabaseName("UK_GroupMenuPermission_GroupId_MenuId_PermissionId_Type");
    }
}
