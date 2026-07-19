using GastroErp.Domain.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Crm;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.CustomerNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("SAR");
        builder.Property(x => x.PaymentTerms).HasMaxLength(200);
        builder.Property(x => x.CreditLimit).HasColumnType("decimal(18,4)");
        builder.Property(x => x.TotalSpending).HasColumnType("decimal(18,4)");
        builder.Property(x => x.AverageTicket).HasColumnType("decimal(18,4)");

        builder.HasIndex(x => new { x.TenantId, x.Mobile }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CustomerNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.CurrentPoints).HasColumnType("decimal(18,4)");
        builder.Property(x => x.EarnedPoints).HasColumnType("decimal(18,4)");
        builder.Property(x => x.RedeemedPoints).HasColumnType("decimal(18,4)");
        builder.Property(x => x.ExpiredPoints).HasColumnType("decimal(18,4)");

        builder.HasOne<Customer>()
            .WithOne(c => c.LoyaltyAccount)
            .HasForeignKey<LoyaltyAccount>(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Tier)
            .WithMany()
            .HasForeignKey(x => x.MembershipTierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.Points).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Reason).HasMaxLength(200);

        builder.HasOne<LoyaltyAccount>()
            .WithMany(l => l.Transactions)
            .HasForeignKey(x => x.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MembershipTierConfiguration : IEntityTypeConfiguration<MembershipTier>
{
    public void Configure(EntityTypeBuilder<MembershipTier> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RequiredPoints).HasColumnType("decimal(18,4)");
        builder.Property(x => x.DiscountPercentage).HasColumnType("decimal(18,4)");
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasColumnType("decimal(18,4)");
        builder.Property(x => x.MinimumOrderAmount).HasColumnType("decimal(18,4)");

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public class PromotionCampaignConfiguration : IEntityTypeConfiguration<PromotionCampaign>
{
    public void Configure(EntityTypeBuilder<PromotionCampaign> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasColumnType("decimal(18,4)");
    }
}

public class GiftCardConfiguration : IEntityTypeConfiguration<GiftCard>
{
    public void Configure(EntityTypeBuilder<GiftCard> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.CardNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.InitialValue).HasColumnType("decimal(18,4)");
        builder.Property(x => x.CurrentBalance).HasColumnType("decimal(18,4)");

        builder.HasIndex(x => new { x.TenantId, x.CardNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
