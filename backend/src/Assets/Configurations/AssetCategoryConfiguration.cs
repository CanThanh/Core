using Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assets.Configurations;

public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.ToTable("AssetCategories");

        builder.HasKey(ac => ac.Id);
        builder.Property(ac => ac.Id).HasColumnType("CHAR(36)");

        builder.Property(ac => ac.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(ac => ac.Name).IsUnique();

        builder.Property(ac => ac.Description).HasMaxLength(255);
    }
}
