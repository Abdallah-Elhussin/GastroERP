using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Ai.DTOs;

public record RecommendationFilterDto(
    RecommendationType? Type = null,
    RecommendationStatus? Status = RecommendationStatus.Pending,
    Guid? BranchId = null,
    int Page = 1,
    int PageSize = 50);

public record RecommendationActionDto(
    Guid Id, RecommendationType Type, RecommendationStatus Status, RecommendationPriority Priority,
    string Title, string Description, string PayloadJson, string ExplainabilityJson,
    Guid? BranchId, Guid? ReferenceId, DateTimeOffset CreatedAt,
    DateTimeOffset? AppliedAt, DateTimeOffset? DismissedAt);

public record PurchaseRecommendationDto(
    Guid InventoryItemId, string ItemName, decimal CurrentStock, decimal SuggestedQty,
    decimal EstimatedCost, Guid? SupplierId, string? SupplierName, int PriorityScore, string Reason);

public record PurchaseRecommendationsResultDto(
    IReadOnlyList<PurchaseRecommendationDto> Items, decimal TotalEstimatedCost, int ItemCount);

public record RecipeCostRecommendationDto(
    Guid RecipeId, Guid ProductId, string RecipeName, decimal CurrentCost, decimal MenuPrice,
    decimal CurrentMarginPercent, decimal TargetMarginPercent, decimal PotentialSavings, string Suggestion);

public record RecipeCostRecommendationsResultDto(IReadOnlyList<RecipeCostRecommendationDto> Items);

public record StaffSchedulingRecommendationDto(
    Guid BranchId, string BranchName, DayOfWeek DayOfWeek, int RecommendedStaff,
    int CurrentAvgStaff, int PeakHourOrders, string AlertLevel, string Suggestion);

public record StaffSchedulingRecommendationsResultDto(IReadOnlyList<StaffSchedulingRecommendationDto> Items);

public record DynamicPricingRecommendationDto(
    Guid ProductId, string ProductName, decimal CurrentPrice, decimal SuggestedPrice,
    double DemandTrendPercent, decimal EstimatedRevenueImpact, string Reason);

public record DynamicPricingRecommendationsResultDto(IReadOnlyList<DynamicPricingRecommendationDto> Items);

public record RefreshRecommendationsDto(
    bool Purchase = true, bool RecipeCost = true, bool StaffScheduling = true, bool DynamicPricing = true);

public record RefreshRecommendationsResultDto(int TotalCreated);

public record ApplyRecommendationDto(string? Notes = null);
public record DismissRecommendationDto(string? Reason = null);
