using Authorization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Entities;

namespace Users.Configurations;

public class GroupPermissionConfiguration : IEntityTypeConfiguration<GroupPermission>
{
    public void Configure(EntityTypeBuilder<GroupPermission> builder)
    {
        builder.ToTable("GroupPermissions");

        builder.HasKey(gp => new { gp.GroupId, gp.PermissionId });

        builder.Property(gp => gp.GroupId)
            .HasColumnType("CHAR(36)");

        builder.Property(gp => gp.PermissionId)
            .HasColumnType("CHAR(36)");

        builder.HasOne(gp => gp.Group)
            .WithMany()
            .HasForeignKey(gp => gp.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gp => gp.Permission)
            .WithMany()
            .HasForeignKey(gp => gp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(gp => gp.GroupId);
        builder.HasIndex(gp => gp.PermissionId);
    }
}
