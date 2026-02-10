using Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assets.Configurations;

public class AssetDepreciationConfiguration : IEntityTypeConfiguration<AssetDepreciation>
{
    public void Configure(EntityTypeBuilder<AssetDepreciation> builder)
    {
        builder.ToTable("AssetDepreciations");

        builder.HasKey(ad => ad.Id);
        builder.Property(ad => ad.Id).HasColumnType("CHAR(36)");

        builder.Property(ad => ad.AssetId).HasColumnType("CHAR(36)").IsRequired();
        builder.Property(ad => ad.Year).IsRequired();
        builder.Property(ad => ad.Month).IsRequired();
        builder.Property(ad => ad.DepreciationAmount).HasColumnType("DECIMAL(18,2)").IsRequired();
        builder.Property(ad => ad.AccumulatedDepreciation).HasColumnType("DECIMAL(18,2)").IsRequired();
        builder.Property(ad => ad.BookValue).HasColumnType("DECIMAL(18,2)").IsRequired();

        builder.HasOne(ad => ad.Asset)
            .WithMany(a => a.Depreciations)
            .HasForeignKey(ad => ad.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ad => new { ad.AssetId, ad.Year, ad.Month }).IsUnique();
    }
}
