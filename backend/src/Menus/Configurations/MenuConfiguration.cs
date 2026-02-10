using Menus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menus.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnType("CHAR(36)");

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Icon)
            .HasMaxLength(50);

        builder.Property(m => m.Route)
            .HasMaxLength(200);

        builder.Property(m => m.DisplayOrder)
            .IsRequired();

        builder.Property(m => m.IsActive)
            .HasDefaultValue(true);

        builder.Property(m => m.ParentId)
            .HasColumnType("CHAR(36)");

        // Self-referencing relationship for hierarchy
        builder.HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.DisplayOrder);
    }
}
