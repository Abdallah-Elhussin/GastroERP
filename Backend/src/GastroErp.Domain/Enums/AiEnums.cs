namespace GastroErp.Domain.Enums;

public enum FeatureEntityType
{
    Branch = 1,
    Product = 2,
    InventoryItem = 3,
    Customer = 4,
    BusinessDay = 5
}

public enum AiFeatureGroup
{
    SalesVelocity = 1,
    Seasonality = 2,
    StockTurnover = 3,
    CustomerRfm = 4,
    KitchenLoad = 5
}

public enum WarehouseSyncStatus
{
    Queued = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4
}

public enum DataQualityLevel
{
    Good = 1,
    Warning = 2,
    Critical = 3
}

public enum MlDatasetFormat
{
    Csv = 1,
    Json = 2
}

public enum MlDatasetSplit
{
    Train = 1,
    Validation = 2,
    Test = 3,
    Full = 4
}

public enum MlDatasetExportStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4
}

public enum ForecastType
{
    Demand = 1,
    Sales = 2,
    Inventory = 3
}

public enum ForecastHorizon
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}

public enum AiModelProvider
{
    Heuristic = 1,
    Internal = 2,
    AzureMl = 3,
    OpenAi = 4
}

public enum PredictionRunStatus
{
    Running = 1,
    Completed = 2,
    Failed = 3
}

public enum StockOutRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum RecommendationType
{
    Purchase = 1,
    RecipeCost = 2,
    StaffScheduling = 3,
    DynamicPricing = 4
}

public enum RecommendationStatus
{
    Pending = 1,
    Applied = 2,
    Dismissed = 3
}

public enum RecommendationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum GenerativeInteractionType
{
    Chat = 1,
    Query = 2,
    Voice = 3,
    Insight = 4
}

public enum VoiceOrderDraftStatus
{
    Draft = 1,
    Confirmed = 2,
    Cancelled = 3
}

public enum FraudType
{
    PaymentFraud = 1,
    DiscountFraud = 2,
    RefundFraud = 3,
    VoidFraud = 4,
    DuplicatePayment = 5
}

public enum FraudSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum FraudAlertStatus
{
    Open = 1,
    Acknowledged = 2,
    Resolved = 3,
    Dismissed = 4
}

public enum CustomerSegmentType
{
    New = 1,
    Active = 2,
    Loyal = 3,
    VIP = 4,
    AtRisk = 5,
    Dormant = 6,
    Lost = 7
}

public enum ChurnRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ProductRecommendationType
{
    Upsell = 1,
    CrossSell = 2,
    SimilarProduct = 3,
    FrequentlyBoughtTogether = 4
}
