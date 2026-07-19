using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class NotificationReasonSeeder : IDataSeeder
{
    private readonly ILogger<NotificationReasonSeeder> _logger;
    public NotificationReasonSeeder(ILogger<NotificationReasonSeeder> logger) => _logger = logger;

    public int Order => 36;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.NotificationReasons.AnyAsync(r => r.TenantId == tenantId, ct))
            return;

        var receivableClassId = await context.AccountClassifications.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Code == "receivable")
            .Select(c => (Guid?)c.Id).FirstOrDefaultAsync(ct);
        var payableClassId = await context.AccountClassifications.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Code == "payable")
            .Select(c => (Guid?)c.Id).FirstOrDefaultAsync(ct);

        var customerAccountId = await FirstPostingAccountAsync(context, tenantId, receivableClassId, ct);
        var supplierAccountId = await FirstPostingAccountAsync(context, tenantId, payableClassId, ct)
            ?? customerAccountId;

        if (customerAccountId is null)
        {
            _logger.LogInformation("Skipping notification reason seed — no posting account for tenant {TenantId}", tenantId);
            return;
        }

        var seeds = new (string Code, string NameAr, string NameEn, NotificationNoteType Note, NotificationPartyType Party, Guid Account)[]
        {
            ("CN-C-DISC", "خصم/حسم للعميل", "Customer discount", NotificationNoteType.Credit, NotificationPartyType.Customer, customerAccountId.Value),
            ("CN-C-PRICE", "فروقات سعر لصالح العميل", "Price difference for customer", NotificationNoteType.Credit, NotificationPartyType.Customer, customerAccountId.Value),
            ("CN-C-RET", "مرتجع للعميل", "Customer return", NotificationNoteType.Credit, NotificationPartyType.Customer, customerAccountId.Value),
            ("DN-C-ADJ", "غرامة تأخير على العميل", "Customer delay fine", NotificationNoteType.Debit, NotificationPartyType.Customer, customerAccountId.Value),
            ("DN-C-DMG", "تعويض تلف على العميل", "Customer damage claim", NotificationNoteType.Debit, NotificationPartyType.Customer, customerAccountId.Value),
            ("CN-V-PRICE", "فروقات سعر لصالح المورد", "Price difference for supplier", NotificationNoteType.Credit, NotificationPartyType.Supplier, supplierAccountId!.Value),
            ("DN-V-PRICE", "مطالبة على المورد", "Claim against supplier", NotificationNoteType.Debit, NotificationPartyType.Supplier, supplierAccountId.Value),
            ("CN-V-RET", "مرتجع للمورد", "Supplier return", NotificationNoteType.Credit, NotificationPartyType.Supplier, supplierAccountId.Value),
        };

        var number = 1;
        foreach (var s in seeds)
        {
            context.NotificationReasons.Add(NotificationReason.Create(
                tenantId, number++, s.Code, s.NameAr, s.Note, s.Party, s.Account, s.NameEn));
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} notification reasons for tenant {TenantId}", seeds.Length, tenantId);
    }

    private static async Task<Guid?> FirstPostingAccountAsync(
        IApplicationDbContext context, Guid tenantId, Guid? classificationId, CancellationToken ct)
    {
        var query = context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.IsActive && a.IsPostingAllowed && !a.IsSummaryAccount);
        if (classificationId.HasValue)
            query = query.Where(a => a.AccountClassificationId == classificationId);
        return await query.OrderBy(a => a.AccountNumber).Select(a => (Guid?)a.Id).FirstOrDefaultAsync(ct);
    }
}
