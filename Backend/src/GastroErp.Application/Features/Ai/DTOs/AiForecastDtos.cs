using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Ai.DTOs;

public record ForecastFilterDto(
    Guid? BranchId = null,
    Guid? ProductId = null,
    Guid? InventoryItemId = null,
    ForecastHorizon Horizon = ForecastHorizon.Daily,
    int DaysAhead = 7);

public record ForecastPeriodDto(
    DateOnly Date, double PredictedValue, double LowerBound, double UpperBound, double Confidence);

public record DemandForecastItemDto(
    Guid ProductId, string ProductName, double AvgDailyQuantity,
    IReadOnlyList<ForecastPeriodDto> Forecast, string Explainability);

public record DemandForecastResultDto(
    Guid? BranchId, DateTimeOffset GeneratedAt, AiModelProvider Provider,
    IReadOnlyList<DemandForecastItemDto> Items);

public record SalesForecastBranchDto(
    Guid BranchId, double HistoricalAvgRevenue, double TrendPercent,
    IReadOnlyList<ForecastPeriodDto> Forecast, string Explainability);

public record SalesForecastResultDto(
    DateTimeOffset GeneratedAt, AiModelProvider Provider,
    IReadOnlyList<SalesForecastBranchDto> Branches, double TotalPredictedRevenue);

public record InventoryForecastItemDto(
    Guid InventoryItemId, string ItemName, decimal CurrentStock,
    double AvgDailyConsumption, int DaysUntilStockout, StockOutRiskLevel RiskLevel,
    decimal SuggestedSafetyStock, string Explainability);

public record InventoryForecastResultDto(
    DateTimeOffset GeneratedAt, AiModelProvider Provider,
    IReadOnlyList<InventoryForecastItemDto> Items, int HighRiskCount);

public record PredictionRunDto(
    Guid Id, ForecastType ForecastType, AiModelProvider Provider,
    PredictionRunStatus Status, DateOnly ForecastDate, double Confidence,
    Guid? BranchId, Guid? EntityId, DateTimeOffset CreatedAt);

public record RefreshForecastsDto(bool Demand = true, bool Sales = true, bool Inventory = true);

public record RefreshForecastsResultDto(
    int DemandItemsForecasted, int SalesBranchesForecasted, int InventoryItemsForecasted);
