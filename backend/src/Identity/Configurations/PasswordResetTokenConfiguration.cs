using Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnType("CHAR(36)");

        builder.Property(p => p.UserId)
            .HasColumnType("CHAR(36)");

        builder.Property(p => p.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.ExpiresAt)
            .IsRequired();

        builder.Property(p => p.IsUsed)
            .IsRequired();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Token);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.ExpiresAt);
    }
}
