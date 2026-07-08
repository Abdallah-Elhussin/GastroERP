using GastroErp.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Organization;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Slug).IsUnique();
        
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        builder.OwnsOne(x => x.Branding, b => 
        {
            b.Property(p => p.LogoUrl).HasMaxLength(500);
            b.Property(p => p.PrimaryColor).HasMaxLength(20);
            b.Property(p => p.SecondaryColor).HasMaxLength(20);
        });
    }
}

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.TaxNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CommercialRegister).HasMaxLength(50);
        builder.Property(x => x.Website).HasMaxLength(200);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        
        builder.OwnsOne(x => x.Address, a => 
        {
            a.Property(p => p.CountryAr).HasMaxLength(100);
            a.Property(p => p.CountryEn).HasMaxLength(100);
            a.Property(p => p.CityAr).HasMaxLength(100);
            a.Property(p => p.CityEn).HasMaxLength(100);
            a.Property(p => p.StreetAr).HasMaxLength(200);
            a.Property(p => p.StreetEn).HasMaxLength(200);
            a.Property(p => p.RegionAr).HasMaxLength(100);
            a.Property(p => p.RegionEn).HasMaxLength(100);
            a.Property(p => p.PostalCode).HasMaxLength(20);
        });
        
        builder.OwnsOne(x => x.Email, e => e.Property(p => p.Value).HasColumnName("Email").HasMaxLength(256));
        builder.OwnsOne(x => x.PhoneNumber, p => p.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(50));
        builder.OwnsOne(x => x.VatNumber, v => v.Property(p => p.Value).HasColumnName("VatNumber").HasMaxLength(15));
        
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.TaxNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(256);
        
        builder.OwnsOne(x => x.Address, a => 
        {
            a.Property(p => p.CountryAr).HasMaxLength(100);
            a.Property(p => p.CountryEn).HasMaxLength(100);
            a.Property(p => p.CityAr).HasMaxLength(100);
            a.Property(p => p.CityEn).HasMaxLength(100);
            a.Property(p => p.StreetAr).HasMaxLength(200);
            a.Property(p => p.StreetEn).HasMaxLength(200);
            a.Property(p => p.RegionAr).HasMaxLength(100);
            a.Property(p => p.RegionEn).HasMaxLength(100);
            a.Property(p => p.PostalCode).HasMaxLength(20);
        });
        
        builder.OwnsOne(x => x.GeoLocation, g => 
        {
            g.Property(p => p.Latitude).HasColumnName("Latitude").HasPrecision(10, 7);
            g.Property(p => p.Longitude).HasColumnName("Longitude").HasPrecision(10, 7);
        });
        
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CompanyId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
    }
}

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Code).HasMaxLength(50);
        
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CompanyId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.BranchId).HasFilter("[IsDeleted] = 0 AND [BranchId] IS NOT NULL");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
    }
}

public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property(x => x.MacAddress).HasMaxLength(50);
        builder.Property(x => x.ActivationCode).IsRequired().HasMaxLength(20);
        
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.ActivationCode).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class BranchDeviceConfiguration : IEntityTypeConfiguration<BranchDevice>
{
    public void Configure(EntityTypeBuilder<BranchDevice> builder)
    {
        builder.ToTable("BranchDevices");
        builder.HasKey(x => new { x.BranchId, x.DeviceId });
        builder.Property(x => x.AssignedBy).HasMaxLength(200);
        builder.Property(x => x.UnassignedBy).HasMaxLength(200);
        
        builder.HasIndex(x => x.TenantId);
    }
}

public sealed class OrganizationSettingsConfiguration : IEntityTypeConfiguration<OrganizationSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationSettings> builder)
    {
        builder.ToTable("OrganizationSettings");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.LegalName).HasMaxLength(200);
        builder.Property(x => x.CommercialRegistration).HasMaxLength(50);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.DateFormat).HasMaxLength(20);
        builder.Property(x => x.NumberFormat).HasMaxLength(20);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.Theme).HasMaxLength(50);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.ContactEmail).HasMaxLength(256);
        builder.Property(x => x.ContactPhone).HasMaxLength(50);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.Code).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class SubscriptionFeatureLimitConfiguration : IEntityTypeConfiguration<SubscriptionFeatureLimit>
{
    public void Configure(EntityTypeBuilder<SubscriptionFeatureLimit> builder)
    {
        builder.ToTable("SubscriptionFeatureLimits");
        builder.HasKey(x => x.Id);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.SubscriptionPlanId, x.FeatureId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.OwnsOne(x => x.MonthlyPrice, p => 
        {
            p.Property(m => m.Amount).HasColumnName("MonthlyPriceAmount").HasColumnType("decimal(18,4)");
            p.Property(m => m.Currency).HasColumnName("MonthlyPriceCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(x => x.YearlyPrice, p => 
        {
            p.Property(m => m.Amount).HasColumnName("YearlyPriceAmount").HasColumnType("decimal(18,4)");
            p.Property(m => m.Currency).HasColumnName("YearlyPriceCurrency").HasMaxLength(3);
        });

        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.OwnsOne(x => x.Price, p => 
        {
            p.Property(m => m.Amount).HasColumnName("PriceAmount").HasColumnType("decimal(18,4)");
            p.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });

        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}
