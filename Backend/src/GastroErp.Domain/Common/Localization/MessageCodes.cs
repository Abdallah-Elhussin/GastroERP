namespace GastroErp.Domain.Common.Localization;

public static class MessageCodes
{
    // Common
    public const string CreatedSuccessfully = "Notification.CreatedSuccessfully";
    public const string UpdatedSuccessfully = "Notification.UpdatedSuccessfully";
    public const string DeletedSuccessfully = "Notification.DeletedSuccessfully";
    
    // Sales / POS
    public const string SalesOrderCreated = "Notification.SalesOrderCreated";
    public const string SalesOrderCancelled = "Notification.SalesOrderCancelled";
    public const string SalesOrderCompleted = "Notification.SalesOrderCompleted";
    public const string SalesOrderConfirmed = "Notification.SalesOrderConfirmed";
    public const string SalesOrderSubmitted = "Notification.SalesOrderSubmitted";

    // Purchasing
    public const string OrderCreated = "Notification.OrderCreated";
    public const string OrderCancelled = "Notification.OrderCancelled";
    
    // Organization
    public const string CompanyActivated = "Notification.CompanyActivated";
    public const string CompanyDeactivated = "Notification.CompanyDeactivated";
}
