using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class RestaurantPaymentMethodSeeder : IDataSeeder
{
    private readonly ILogger<RestaurantPaymentMethodSeeder> _logger;

    public RestaurantPaymentMethodSeeder(ILogger<RestaurantPaymentMethodSeeder> logger) => _logger = logger;

    public int Order => 35;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.TenantPaymentMethods.AnyAsync(p => p.TenantId == tenantId, ct))
        {
            return;
        }

        var methods = new (PaymentMethodType Type, string Ar, string En)[]
        {
            (PaymentMethodType.Cash, "نقدي", "Cash"),
            (PaymentMethodType.CreditCard, "فيزا", "Visa"),
            (PaymentMethodType.DebitCard, "ماستركارد", "MasterCard"),
            (PaymentMethodType.Mada, "مدى", "Mada"),
            (PaymentMethodType.ApplePay, "Apple Pay", "Apple Pay"),
            (PaymentMethodType.STCPay, "STC Pay", "STC Pay"),
            (PaymentMethodType.BankTransfer, "تحويل بنكي", "Bank Transfer")
        };

        for (var i = 0; i < methods.Length; i++)
        {
            var (type, ar, en) = methods[i];
            context.TenantPaymentMethods.Add(new TenantPaymentMethod(tenantId, type, ar, en, i + 1));
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Payment methods seeded for tenant {TenantId}", tenantId);
    }
}
