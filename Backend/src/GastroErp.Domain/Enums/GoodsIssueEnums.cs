namespace GastroErp.Domain.Enums;

/// <summary>حالة مستند الصرف المخزني.</summary>
public enum GoodsIssueStatus : byte
{
    Draft = 1,
    Approved = 2,
    Posted = 3,
    Cancelled = 4
}
