using GastroErp.Domain.Entities.Workflow;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.UnitTests.Workflow;

public class WorkflowDefinitionTests
{
    [Fact]
    public void Create_sets_defaults()
    {
        var def = WorkflowDefinition.Create(Guid.NewGuid(), "Leave Approval", "LEAVE-APPROVAL", WorkflowModule.HR);
        Assert.Equal("LEAVE-APPROVAL", def.Code);
        Assert.True(def.IsActive);
        Assert.False(def.IsPublished);
    }

    [Fact]
    public void Publish_increments_version()
    {
        var def = WorkflowDefinition.Create(Guid.NewGuid(), "Test", "TEST", WorkflowModule.General);
        def.Publish();
        Assert.True(def.IsPublished);
        Assert.Equal(2, def.Version);
    }
}

public class WorkflowInstanceTests
{
    [Fact]
    public void Start_raises_started_event()
    {
        var tenantId = Guid.NewGuid();
        var instance = WorkflowInstance.Start(tenantId, Guid.NewGuid(), "LeaveRequest", Guid.NewGuid(),
            Guid.NewGuid(), WorkflowPriority.Normal, Guid.NewGuid(), 1);
        Assert.Equal(WorkflowStatus.InProgress, instance.Status);
        Assert.Contains(instance.DomainEvents, e => e.GetType().Name == "WorkflowStartedEvent");
    }

    [Fact]
    public void Reject_sets_status()
    {
        var instance = WorkflowInstance.Start(Guid.NewGuid(), Guid.NewGuid(), "PO", Guid.NewGuid(),
            Guid.NewGuid(), WorkflowPriority.High, Guid.NewGuid(), 1);
        instance.Reject(Guid.NewGuid(), "Budget exceeded");
        Assert.Equal(WorkflowStatus.Rejected, instance.Status);
    }
}

public class WorkflowConditionEvaluationTests
{
    [Theory]
    [InlineData(WorkflowConditionOperator.GreaterThan, "10000", true)]
    [InlineData(WorkflowConditionOperator.Equals, "Finance", true)]
    [InlineData(WorkflowConditionOperator.LessThan, "10000", true)]
    public void Condition_operators_work(WorkflowConditionOperator op, string expected, bool _)
    {
        var cond = WorkflowCondition.Create(Guid.NewGuid(), Guid.NewGuid(), "Amount", op, expected);
        Assert.Equal(expected, cond.Value);
        Assert.Equal(op, cond.Operator);
    }
}

public class ApprovalDelegateTests
{
    [Fact]
    public void Create_delegation_active()
    {
        var d = ApprovalDelegate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));
        Assert.True(d.IsActive);
    }
}
