using GastroErp.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Menus;

/// <summary>إعداد EF Core لكيان Category</summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NameEn).HasMaxLength(200);
        builder.Property(c => c.DescriptionAr).HasMaxLength(500);
        builder.Property(c => c.DescriptionEn).HasMaxLength(500);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.Color).HasMaxLength(7);
        builder.Property(c => c.Icon).HasMaxLength(100);
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedBy).HasMaxLength(200);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // Soft delete global filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Self-referencing hierarchy
        builder.HasOne(c => c.ParentCategory)
               .WithMany(c => c.SubCategories)
               .HasForeignKey(c => c.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.TenantId, c.SortOrder })
               .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => c.TenantId)
               .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.NameAr })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان Product</summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(p => p.NameEn).HasMaxLength(200);
        builder.Property(p => p.DescriptionAr).HasMaxLength(1000);
        builder.Property(p => p.DescriptionEn).HasMaxLength(1000);
        builder.Property(p => p.SKU).HasMaxLength(50);
        builder.Property(p => p.Barcode).HasMaxLength(50);
        builder.Property(p => p.BasePrice).HasPrecision(18, 4);
        builder.Property(p => p.Currency).HasMaxLength(3);
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(p => !p.IsDeleted);

        // ModifierGroups → Owned collection (separate table)
        builder.HasMany(p => p.ModifierGroups)
               .WithOne()
               .HasForeignKey("ProductId")
               .OnDelete(DeleteBehavior.Restrict);

        // OptionGroups → Owned collection
        builder.HasMany(p => p.OptionGroups)
               .WithOne()
               .HasForeignKey("ProductId")
               .OnDelete(DeleteBehavior.Restrict);

        // Images
        builder.HasMany(p => p.Images)
               .WithOne()
               .HasForeignKey(img => img.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        // PriceLevels (join table)
        builder.HasMany(p => p.PriceLevels)
               .WithOne()
               .HasForeignKey(pl => pl.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        // Unique SKU per tenant (filtered — SKU is optional)
        builder.HasIndex(p => new { p.TenantId, p.SKU })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0 AND [SKU] IS NOT NULL");

        builder.HasIndex(p => p.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => p.CategoryId).HasFilter("[IsDeleted] = 0");
        builder.ToTable(tb => tb.HasCheckConstraint("CK_Products_BasePrice", "[BasePrice] >= 0"));
    }
}

/// <summary>إعداد EF Core لكيان ModifierGroup</summary>
public sealed class ModifierGroupConfiguration : IEntityTypeConfiguration<ModifierGroup>
{
    public void Configure(EntityTypeBuilder<ModifierGroup> builder)
    {
        builder.ToTable("ModifierGroups");
        builder.HasKey(mg => mg.Id);

        builder.Property(mg => mg.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(mg => mg.NameEn).HasMaxLength(200);
        builder.Property(mg => mg.CreatedBy).HasMaxLength(200);
        builder.Property(mg => mg.UpdatedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(mg => !mg.IsDeleted);

        builder.HasMany(mg => mg.Modifiers)
               .WithOne()
               .HasForeignKey("ModifierGroupId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(tb => tb.HasCheckConstraint(
            "CK_ModifierGroups_Selection",
            "[MaxSelection] >= [MinSelection] AND [MaxSelection] >= 1"));

        builder.HasIndex(mg => mg.ProductId).HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان Modifier</summary>
public sealed class ModifierConfiguration : IEntityTypeConfiguration<Modifier>
{
    public void Configure(EntityTypeBuilder<Modifier> builder)
    {
        builder.ToTable("Modifiers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(m => m.NameEn).HasMaxLength(200);
        builder.Property(m => m.ExtraPrice).HasPrecision(18, 4);
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.UpdatedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(m => !m.IsDeleted);

        builder.ToTable(tb => tb.HasCheckConstraint(
            "CK_Modifiers_ExtraPrice", "[ExtraPrice] >= 0"));

        builder.HasIndex(m => m.ModifierGroupId).HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان OptionGroup</summary>
public sealed class OptionGroupConfiguration : IEntityTypeConfiguration<OptionGroup>
{
    public void Configure(EntityTypeBuilder<OptionGroup> builder)
    {
        builder.ToTable("OptionGroups");
        builder.HasKey(og => og.Id);

        builder.Property(og => og.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(og => og.NameEn).HasMaxLength(200);
        builder.Property(og => og.CreatedBy).HasMaxLength(200);
        builder.Property(og => og.UpdatedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(og => !og.IsDeleted);

        builder.HasMany(og => og.Options)
               .WithOne()
               .HasForeignKey("OptionGroupId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(og => og.ProductId).HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان Option</summary>
public sealed class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.ToTable("Options");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(o => o.NameEn).HasMaxLength(200);
        builder.Property(o => o.ExtraPrice).HasPrecision(18, 4);
        builder.Property(o => o.CreatedBy).HasMaxLength(200);
        builder.Property(o => o.UpdatedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(o => !o.IsDeleted);
        builder.ToTable(tb => tb.HasCheckConstraint("CK_Options_ExtraPrice", "[ExtraPrice] >= 0"));
    }
}

/// <summary>إعداد EF Core لكيان ProductImage</summary>
public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(pi => pi.ThumbnailUrl).HasMaxLength(500);
        builder.Property(pi => pi.AltText).HasMaxLength(200);
        builder.Property(pi => pi.CreatedBy).HasMaxLength(200);

        builder.HasIndex(pi => pi.ProductId);
    }
}

/// <summary>إعداد EF Core لكيان ProductPriceLevel</summary>
public sealed class ProductPriceLevelConfiguration : IEntityTypeConfiguration<ProductPriceLevel>
{
    public void Configure(EntityTypeBuilder<ProductPriceLevel> builder)
    {
        builder.ToTable("ProductPriceLevels");
        builder.HasKey(ppl => new { ppl.ProductId, ppl.PriceLevelId });

        builder.Property(ppl => ppl.Price).HasPrecision(18, 4);

        builder.ToTable(tb => tb.HasCheckConstraint("CK_ProductPriceLevels_Price", "[Price] >= 0"));
    }
}

/// <summary>إعداد EF Core لكيان PriceLevel</summary>
public sealed class PriceLevelConfiguration : IEntityTypeConfiguration<PriceLevel>
{
    public void Configure(EntityTypeBuilder<PriceLevel> builder)
    {
        builder.ToTable("PriceLevels");
        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(pl => pl.NameEn).HasMaxLength(100);
        builder.Property(pl => pl.SalesChannel).HasConversion<byte>();
        builder.Property(pl => pl.CreatedBy).HasMaxLength(200);
        builder.Property(pl => pl.UpdatedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(pl => !pl.IsDeleted);
        builder.HasIndex(pl => pl.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(pl => new { pl.TenantId, pl.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان Menu</summary>
public sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(m => m.NameEn).HasMaxLength(200);
        builder.Property(m => m.DescriptionAr).HasMaxLength(500);
        builder.Property(m => m.DescriptionEn).HasMaxLength(500);
        builder.Property(m => m.MenuType).HasConversion<byte>();
        builder.Property(m => m.SalesChannel).HasConversion<byte>();
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.UpdatedBy).HasMaxLength(200);
        builder.Property(m => m.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(m => !m.IsDeleted);

        builder.HasMany(m => m.Sections)
               .WithOne()
               .HasForeignKey("MenuId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(m => new { m.TenantId, m.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان MenuSection</summary>
public sealed class MenuSectionConfiguration : IEntityTypeConfiguration<MenuSection>
{
    public void Configure(EntityTypeBuilder<MenuSection> builder)
    {
        builder.ToTable("MenuSections");
        builder.HasKey(ms => ms.Id);

        builder.Property(ms => ms.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(ms => ms.NameEn).HasMaxLength(200);
        builder.Property(ms => ms.DescriptionAr).HasMaxLength(500);
        builder.Property(ms => ms.DescriptionEn).HasMaxLength(500);
        builder.Property(ms => ms.ImageUrl).HasMaxLength(500);
        builder.Property(ms => ms.CreatedBy).HasMaxLength(200);
        builder.Property(ms => ms.UpdatedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(ms => !ms.IsDeleted);

        builder.HasMany(ms => ms.Items)
               .WithOne()
               .HasForeignKey("MenuSectionId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ms => ms.MenuId).HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان MenuItem</summary>
public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.OverridePrice).HasPrecision(18, 4);
        builder.Property(mi => mi.CreatedBy).HasMaxLength(200);
        builder.Property(mi => mi.UpdatedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(mi => !mi.IsDeleted);

        builder.HasIndex(mi => new { mi.MenuSectionId, mi.ProductId })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(mi => mi.ProductId).HasFilter("[IsDeleted] = 0");
        builder.ToTable(tb => tb.HasCheckConstraint("CK_MenuItems_OverridePrice", "[OverridePrice] IS NULL OR [OverridePrice] >= 0"));
    }
}

/// <summary>إعداد EF Core لكيان BranchMenu</summary>
public sealed class BranchMenuConfiguration : IEntityTypeConfiguration<BranchMenu>
{
    public void Configure(EntityTypeBuilder<BranchMenu> builder)
    {
        builder.ToTable("BranchMenus");
        builder.HasKey(bm => bm.Id);

        builder.Property(bm => bm.CreatedBy).HasMaxLength(200);
        builder.Property(bm => bm.UpdatedBy).HasMaxLength(200);

        builder.HasMany(bm => bm.Availabilities)
               .WithOne()
               .HasForeignKey(a => a.BranchMenuId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bm => new { bm.BranchId, bm.MenuId })
               .IsUnique();
        builder.HasIndex(bm => bm.BranchId);
    }
}

/// <summary>إعداد EF Core لكيان MenuAvailability</summary>
public sealed class MenuAvailabilityConfiguration : IEntityTypeConfiguration<MenuAvailability>
{
    public void Configure(EntityTypeBuilder<MenuAvailability> builder)
    {
        builder.ToTable("MenuAvailabilities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.DayOfWeek).HasConversion<byte>();

        builder.HasIndex(a => new { a.BranchMenuId, a.DayOfWeek }).IsUnique();
    }
}

/// <summary>إعداد EF Core لكيان ComboMeal</summary>
public sealed class ComboMealConfiguration : IEntityTypeConfiguration<ComboMeal>
{
    public void Configure(EntityTypeBuilder<ComboMeal> builder)
    {
        builder.ToTable("ComboMeals");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NameEn).HasMaxLength(200);
        builder.Property(c => c.DescriptionAr).HasMaxLength(500);
        builder.Property(c => c.DescriptionEn).HasMaxLength(500);
        builder.Property(c => c.ComboPrice).HasPrecision(18, 4);
        builder.Property(c => c.Currency).HasMaxLength(3);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedBy).HasMaxLength(200);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasMany(c => c.Items)
               .WithOne()
               .HasForeignKey(ci => ci.ComboMealId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(tb => {
            tb.HasCheckConstraint("CK_ComboMeals_Price", "[ComboPrice] >= 0");
            tb.HasCheckConstraint("CK_ComboMeals_Dates", "[EndDate] IS NULL OR [EndDate] >= [StartDate]");
        });

        builder.HasIndex(c => c.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

/// <summary>إعداد EF Core لكيان ComboItem</summary>
public sealed class ComboItemConfiguration : IEntityTypeConfiguration<ComboItem>
{
    public void Configure(EntityTypeBuilder<ComboItem> builder)
    {
        builder.ToTable("ComboItems");
        builder.HasKey(ci => ci.Id);

        builder.ToTable(tb => tb.HasCheckConstraint("CK_ComboItems_Quantity", "[Quantity] >= 1"));
        builder.HasIndex(ci => ci.ComboMealId);
    }
}
