using Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assets.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnType("CHAR(36)");

        builder.Property(a => a.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.CategoryId).HasColumnType("CHAR(36)").IsRequired();
        builder.Property(a => a.Manufacturer).HasMaxLength(100);
        builder.Property(a => a.SerialNumber).HasMaxLength(100);
        builder.Property(a => a.PurchasePrice).HasColumnType("DECIMAL(18,2)").IsRequired();
        builder.Property(a => a.DepreciationRate).HasColumnType("DECIMAL(5,2)").IsRequired();
        builder.Property(a => a.Location).HasMaxLength(255);
        builder.Property(a => a.Status).IsRequired().HasMaxLength(20);
        builder.Property(a => a.IsActive).HasDefaultValue(true);

        builder.HasOne(a => a.Category)
            .WithMany(ac => ac.Assets)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CategoryId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.IsActive);
    }
}
