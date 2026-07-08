using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Workflow;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>تعريفات Workflow الافتراضية لجميع الوحدات المربوطة.</summary>
public sealed class WorkflowDefinitionsSeeder : IDataSeeder
{
    private readonly ILogger<WorkflowDefinitionsSeeder> _logger;

    public WorkflowDefinitionsSeeder(ILogger<WorkflowDefinitionsSeeder> logger) => _logger = logger;

    public int Order => 50;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.LeaveApproval, "اعتماد الإجازة", "Leave Approval",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير مباشر", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.PayrollApproval, "اعتماد الرواتب", "Payroll Approval",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير الموارد البشرية", ApprovalType.RoleBased, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.OvertimeApproval, "اعتماد العمل الإضافي", "Overtime Approval",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير مباشر", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.LoanApproval, "اعتماد القرض", "Loan Approval",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير مباشر", ApprovalType.Manager, false), ("المالية", ApprovalType.RoleBased, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.SalaryAdvanceApproval, "اعتماد السلفة", "Salary Advance",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير مباشر", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.ResignationApproval, "اعتماد الاستقالة", "Resignation",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير مباشر", ApprovalType.Manager, false), ("الموارد البشرية", ApprovalType.RoleBased, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.PromotionApproval, "اعتماد الترقية", "Promotion",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير عام", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.TransferApproval, "اعتماد النقل", "Transfer",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير مباشر", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.PerformanceApproval, "اعتماد التقييم", "Performance Review",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير مباشر", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.RecruitmentApproval, "اعتماد التوظيف", "Recruitment",
            WorkflowModule.HR, WorkflowTrigger.OnSubmit,
            [("مدير التوظيف", ApprovalType.RoleBased, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.PurchaseOrderApproval, "اعتماد أمر الشراء", "PO Approval",
            WorkflowModule.Purchasing, WorkflowTrigger.OnSubmit,
            [("مدير المشتريات", ApprovalType.Manager, false), ("المالية", ApprovalType.RoleBased, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.StockCountApproval, "اعتماد الجرد", "Stock Count",
            WorkflowModule.Inventory, WorkflowTrigger.OnSubmit,
            [("مدير المستودع", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.StockAdjustmentApproval, "اعتماد التسوية", "Stock Adjustment",
            WorkflowModule.Inventory, WorkflowTrigger.OnSubmit,
            [("مدير المستودع", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.StockTransferApproval, "اعتماد النقل المخزني", "Stock Transfer",
            WorkflowModule.Inventory, WorkflowTrigger.OnSubmit,
            [("مدير المستودع", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.PosRefundApproval, "اعتماد الاسترجاع", "POS Refund",
            WorkflowModule.Sales, WorkflowTrigger.OnSubmit,
            [("مشرف الصالة", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.JournalApproval, "اعتماد القيد", "Journal Approval",
            WorkflowModule.Finance, WorkflowTrigger.OnSubmit,
            [("محاسب", ApprovalType.RoleBased, false), ("مدير مالي", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.ExpenseApproval, "اعتماد المصروف", "Expense Approval",
            WorkflowModule.Finance, WorkflowTrigger.OnSubmit,
            [("مدير مالي", ApprovalType.Manager, true)], ct);

        await SeedDefinitionAsync(tenantId, context,
            WorkflowIntegrationCodes.DiscountApproval, "اعتماد الخصم", "Discount Override",
            WorkflowModule.Sales, WorkflowTrigger.Manual,
            [("مشرف", ApprovalType.Manager, true)], ct);

        _logger.LogInformation("Workflow definitions seeded for tenant {TenantId}", tenantId);
    }

    private static async Task SeedDefinitionAsync(
        Guid tenantId, IApplicationDbContext context, string code, string nameAr, string nameEn,
        WorkflowModule module, WorkflowTrigger trigger,
        (string StepName, ApprovalType Type, bool IsFinal)[] steps,
        CancellationToken ct)
    {
        if (await context.WorkflowDefinitions.AnyAsync(
            w => w.TenantId == tenantId && w.Code == code && w.IsPublished, ct))
            return;

        var def = WorkflowDefinition.Create(tenantId, nameAr, code, module, nameEn, trigger, WorkflowPriority.Normal);
        context.WorkflowDefinitions.Add(def);
        await context.SaveChangesAsync(ct);

        for (var i = 0; i < steps.Length; i++)
        {
            var (stepName, approvalType, isFinal) = steps[i];
            context.WorkflowSteps.Add(WorkflowStep.Create(
                tenantId, def.Id, i + 1, stepName, approvalType, isFinal));
        }

        def.Publish();
        def.Activate();
        await context.SaveChangesAsync(ct);
    }
}
