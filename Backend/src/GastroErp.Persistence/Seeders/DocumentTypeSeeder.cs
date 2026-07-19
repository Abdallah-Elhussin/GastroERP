using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>أنواع مستندات افتراضية لكل وحدات المطعم.</summary>
public sealed class DocumentTypeSeeder : IDataSeeder
{
    private readonly ILogger<DocumentTypeSeeder> _logger;
    public DocumentTypeSeeder(ILogger<DocumentTypeSeeder> logger) => _logger = logger;
    public int Order => 23;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.DocumentTypes.AnyAsync(d => d.TenantId == tenantId, ct))
            return;

        var order = 1;
        void Add(
            string code,
            string ar,
            string en,
            DocumentModule module,
            string prefix,
            bool approval = false,
            bool autoPost = false,
            bool postAfterApproval = false,
            bool inventory = false,
            bool accounting = false,
            bool cash = false,
            bool customers = false,
            bool suppliers = false,
            bool payroll = false,
            bool cost = false)
        {
            var doc = DocumentType.Create(tenantId, code, ar, en, module, prefix, sortOrder: order++, isSystem: true);
            doc.UpdateApproval(
                approval ? DocumentApprovalMode.Single : DocumentApprovalMode.None,
                approval, usesWorkflow: approval, workflowDefinitionId: null);
            doc.UpdatePosting(
                autoPost ? DocumentPostingMode.AutoPost
                    : postAfterApproval ? DocumentPostingMode.PostAfterApproval
                    : DocumentPostingMode.Manual,
                autoPost, postAfterApproval);
            doc.UpdateImpact(inventory, cost, accounting, cash, customers, suppliers, assets: false, payroll);
            if (approval) doc.UpdateExtras(true, true, true, false, true, true, false, true, false);
            context.DocumentTypes.Add(doc);
        }

        // Inventory
        Add("GRN", "إذن استلام", "Goods Receipt", DocumentModule.Inventory, "GRN", inventory: true, cost: true, accounting: true);
        Add("GI", "إذن صرف", "Goods Issue", DocumentModule.Inventory, "GI", approval: true, inventory: true, cost: true, accounting: true);
        Add("ST", "تحويل مخزني", "Stock Transfer", DocumentModule.Inventory, "ST", inventory: true);
        Add("CNT", "جرد", "Stock Count", DocumentModule.Inventory, "CNT", approval: true, inventory: true);
        Add("ADJ", "تسوية مخزون", "Stock Adjustment", DocumentModule.Inventory, "ADJ", approval: true, inventory: true, cost: true, accounting: true);

        // Purchasing
        Add("PR", "طلب شراء", "Purchase Requisition", DocumentModule.Purchasing, "PR", approval: true);
        Add("PO", "أمر شراء", "Purchase Order", DocumentModule.Purchasing, "PO", approval: true, suppliers: true);
        Add("PINV", "فاتورة شراء", "Purchase Invoice", DocumentModule.Purchasing, "PINV", approval: true, postAfterApproval: true, accounting: true, suppliers: true, cost: true);
        Add("PRTN", "مرتجع شراء", "Purchase Return", DocumentModule.Purchasing, "PRTN", approval: true, inventory: true, accounting: true, suppliers: true);

        // Sales
        Add("QT", "عرض سعر", "Quotation", DocumentModule.Sales, "QT", customers: true);
        Add("SO", "أمر بيع", "Sales Order", DocumentModule.Sales, "SO", customers: true);
        Add("SINV", "فاتورة بيع", "Sales Invoice", DocumentModule.Sales, "SINV", autoPost: true, accounting: true, customers: true, cash: true);
        Add("SRTN", "مرتجع بيع", "Sales Return", DocumentModule.Sales, "SRTN", approval: true, inventory: true, accounting: true, customers: true);

        // Finance
        Add("JE", "قيد يومية", "Journal Entry", DocumentModule.Finance, "JE", approval: true, postAfterApproval: true, accounting: true);
        Add("RV", "سند قبض", "Receipt Voucher", DocumentModule.Finance, "RV", accounting: true, cash: true, customers: true);
        Add("PV", "سند صرف", "Payment Voucher", DocumentModule.Finance, "PV", approval: true, accounting: true, cash: true, suppliers: true);
        Add("CN", "إشعار دائن", "Credit Note", DocumentModule.Finance, "CN", accounting: true, customers: true);
        Add("DN", "إشعار مدين", "Debit Note", DocumentModule.Finance, "DN", accounting: true, suppliers: true);

        // HR
        Add("LV", "طلب إجازة", "Leave Request", DocumentModule.Hr, "LV", approval: true);
        Add("ADV", "طلب سلفة", "Advance Request", DocumentModule.Hr, "ADV", approval: true, payroll: true);
        Add("PAY", "كشف راتب", "Payslip", DocumentModule.Hr, "PAY", accounting: true, payroll: true);
        Add("ATT", "حركة حضور", "Attendance", DocumentModule.Hr, "ATT");

        // Production
        Add("MO", "أمر إنتاج", "Production Order", DocumentModule.Production, "MO", approval: true, inventory: true);
        Add("MOR", "استلام إنتاج", "Production Receipt", DocumentModule.Production, "MOR", inventory: true, cost: true);
        Add("MIC", "استهلاك مواد", "Material Consumption", DocumentModule.Production, "MIC", inventory: true, cost: true);

        // Maintenance
        Add("WO", "أمر صيانة", "Work Order", DocumentModule.Maintenance, "WO", approval: true);
        Add("MR", "طلب صيانة", "Maintenance Request", DocumentModule.Maintenance, "MR", approval: true);

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Document types seeded for tenant {TenantId}", tenantId);
    }
}
