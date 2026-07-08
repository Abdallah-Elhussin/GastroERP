namespace GastroErp.Domain.ValueObjects;

/// <summary>
/// كائن قيمة يمثل العنوان الكامل
/// </summary>
public sealed record Address
{
    public string? StreetAr { get; }
    public string? StreetEn { get; }
    public string? CityAr { get; }
    public string? CityEn { get; }
    public string? RegionAr { get; }
    public string? RegionEn { get; }
    public string? PostalCode { get; }
    public string CountryAr { get; }
    public string CountryEn { get; }

    private Address() 
    { 
        CountryAr = "المملكة العربية السعودية"; 
        CountryEn = "Saudi Arabia"; 
    }

    public Address(string? streetAr, string? streetEn, string? cityAr, string? cityEn, string? regionAr, string? regionEn, string? postalCode, string countryAr = "المملكة العربية السعودية", string countryEn = "Saudi Arabia")
    {
        if (string.IsNullOrWhiteSpace(countryAr))
            throw new ArgumentException("CountryAr cannot be empty.", nameof(countryAr));
        if (string.IsNullOrWhiteSpace(countryEn))
            throw new ArgumentException("CountryEn cannot be empty.", nameof(countryEn));

        StreetAr = streetAr;
        StreetEn = streetEn;
        CityAr = cityAr;
        CityEn = cityEn;
        RegionAr = regionAr;
        RegionEn = regionEn;
        PostalCode = postalCode;
        CountryAr = countryAr;
        CountryEn = countryEn;
    }

    public static Address Empty => new(null, null, null, null, null, null, null, "المملكة العربية السعودية", "Saudi Arabia");
    public string FullAddressAr => string.Join("، ", new[] { StreetAr, CityAr, RegionAr, CountryAr }.Where(x => !string.IsNullOrWhiteSpace(x)));
    public string FullAddressEn => string.Join(", ", new[] { StreetEn, CityEn, RegionEn, CountryEn }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

/// <summary>
/// كائن قيمة يمثل الموقع الجغرافي
/// </summary>
public sealed record GeoLocation
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    private GeoLocation() { }

    public GeoLocation(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }
}

/// <summary>
/// كائن قيمة يمثل الهوية البصرية للمستأجر (Branding)
/// </summary>
public sealed record TenantBranding
{
    public string? LogoUrl { get; }
    public string? PrimaryColor { get; }
    public string? SecondaryColor { get; }

    private TenantBranding() { }

    public TenantBranding(string? logoUrl, string? primaryColor, string? secondaryColor)
    {
        if (primaryColor is not null && (primaryColor.Length != 7 || !primaryColor.StartsWith('#')))
            throw new ArgumentException("Primary color must be a valid HEX color (e.g. #FF5733).", nameof(primaryColor));

        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }

    public static TenantBranding Empty => new(null, null, null);
}

/// <summary>
/// كائن قيمة يمثل مبلغ مالي مع عملته
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { Currency = "SAR"; }

    public Money(decimal amount, string currency = "SAR")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a valid 3-letter ISO code.", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "SAR") => new(0, currency);
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add amounts with different currencies.");
        return new Money(Amount + other.Amount, Currency);
    }
}
