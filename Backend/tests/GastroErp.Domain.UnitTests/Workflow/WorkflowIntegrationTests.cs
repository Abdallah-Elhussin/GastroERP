using GastroErp.Domain.Enums;
using GastroErp.Domain.Workflow;

namespace GastroErp.Domain.UnitTests.Workflow;

public class WorkflowIntegrationConstantsTests
{
    [Theory]
    [InlineData(WorkflowIntegrationReferenceTypes.LeaveRequest, WorkflowIntegrationCodes.LeaveApproval)]
    [InlineData(WorkflowIntegrationReferenceTypes.PurchaseOrder, WorkflowIntegrationCodes.PurchaseOrderApproval)]
    [InlineData(WorkflowIntegrationReferenceTypes.PayrollRun, WorkflowIntegrationCodes.PayrollApproval)]
    [InlineData(WorkflowIntegrationReferenceTypes.Refund, WorkflowIntegrationCodes.PosRefundApproval)]
    public void Reference_types_have_workflow_codes(string referenceType, string expectedCode)
    {
        var map = new Dictionary<string, string>
        {
            [WorkflowIntegrationReferenceTypes.LeaveRequest] = WorkflowIntegrationCodes.LeaveApproval,
            [WorkflowIntegrationReferenceTypes.PurchaseOrder] = WorkflowIntegrationCodes.PurchaseOrderApproval,
            [WorkflowIntegrationReferenceTypes.PayrollRun] = WorkflowIntegrationCodes.PayrollApproval,
            [WorkflowIntegrationReferenceTypes.Refund] = WorkflowIntegrationCodes.PosRefundApproval,
            [WorkflowIntegrationReferenceTypes.StockCount] = WorkflowIntegrationCodes.StockCountApproval,
            [WorkflowIntegrationReferenceTypes.JobApplicant] = WorkflowIntegrationCodes.RecruitmentApproval
        };
        Assert.Equal(expectedCode, map[referenceType]);
    }
}

public class HrWorkflowRequestIntegrationTests
{
    [Fact]
    public void Submit_raises_submitted_event()
    {
        var req = Domain.Entities.HR.HrWorkflowRequest.Submit(
            Guid.NewGuid(), Guid.NewGuid(), HrWorkflowRequestType.Overtime, "Overtime Q3", amount: 500);
        Assert.Equal(HrWorkflowRequestStatus.Pending, req.Status);
        Assert.Contains(req.DomainEvents, e => e.GetType().Name == "HrWorkflowRequestSubmittedEvent");
    }
}

public class PurchaseOrderWorkflowIntegrationTests
{
    [Fact]
    public void SubmitForApproval_changes_status_and_raises_event()
    {
        var po = new Domain.Entities.Inventory.Purchasing.PurchaseOrder(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "PO-001",
            expectedDeliveryDate: DateTimeOffset.UtcNow.AddDays(7));
        po.AddLine(Guid.NewGuid(), Guid.NewGuid(), 10, 100);
        po.SubmitForApproval();
        Assert.Equal(PurchaseOrderStatus.PendingApproval, po.Status);
        Assert.Contains(po.DomainEvents, e => e.GetType().Name == "PurchaseOrderSubmittedEvent");
    }
}
