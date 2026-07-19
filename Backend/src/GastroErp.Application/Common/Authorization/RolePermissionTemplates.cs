namespace GastroErp.Application.Common.Authorization;

public static class RolePermissionTemplates
{
    private static readonly HashSet<string> ViewPrefixes =
    [
        "Organization.View", "Tenant.View", "Company.View", "Branch.View",
        "Menu.View", "Category.View", "Product.View", "Inventory.View",
        "Sales.View", "Reporting.View", "Finance.View", "HR.View"
    ];

    public static bool MatchesRole(string roleName, string permissionName) =>
        roleName switch
        {
            "Administrator" => true,
            "Branch Manager" => IsBranchManagerPermission(permissionName),
            "Cashier" => IsCashierPermission(permissionName),
            "Waiter" => IsWaiterPermission(permissionName),
            "Kitchen" => IsKitchenPermission(permissionName),
            "Inventory" => IsInventoryPermission(permissionName),
            "Accountant" => IsAccountantPermission(permissionName),
            "HR" => IsHrPermission(permissionName),
            _ => permissionName.EndsWith(".View", StringComparison.Ordinal)
                 || permissionName.EndsWith(".Use", StringComparison.Ordinal)
        };

    private static bool IsBranchManagerPermission(string permission) =>
        permission.StartsWith("Organization.", StringComparison.Ordinal)
        || permission.StartsWith("Branch.", StringComparison.Ordinal)
        || permission.StartsWith("Sales.", StringComparison.Ordinal)
        || permission.StartsWith("BackOfficeSales.", StringComparison.Ordinal)
        || permission.StartsWith("Crm.", StringComparison.Ordinal)
        || permission.StartsWith("Inventory.", StringComparison.Ordinal)
        || permission.StartsWith("Menu.", StringComparison.Ordinal)
        || permission.StartsWith("Reporting.", StringComparison.Ordinal)
        || permission.StartsWith("HR.", StringComparison.Ordinal);

    private static bool IsCashierPermission(string permission) =>
        permission.StartsWith("Sales.", StringComparison.Ordinal)
        || permission.StartsWith("Menu.View", StringComparison.Ordinal)
        || permission.StartsWith("Product.View", StringComparison.Ordinal)
        || permission.Equals("Crm.View", StringComparison.Ordinal);

    private static bool IsWaiterPermission(string permission) =>
        permission.StartsWith("Sales.", StringComparison.Ordinal)
        || permission.StartsWith("Menu.View", StringComparison.Ordinal)
        || permission.StartsWith("Kitchen.View", StringComparison.Ordinal);

    private static bool IsKitchenPermission(string permission) =>
        permission.StartsWith("Kitchen.", StringComparison.Ordinal)
        || permission.StartsWith("Sales.View", StringComparison.Ordinal)
        || permission.StartsWith("Inventory.View", StringComparison.Ordinal);

    private static bool IsInventoryPermission(string permission) =>
        permission.StartsWith("Inventory.", StringComparison.Ordinal)
        || permission.StartsWith("Purchasing.", StringComparison.Ordinal);

    private static bool IsAccountantPermission(string permission) =>
        permission.StartsWith("Finance.", StringComparison.Ordinal)
        || permission.StartsWith("Invoicing.", StringComparison.Ordinal)
        || permission.StartsWith("BackOfficeSales.", StringComparison.Ordinal)
        || permission.StartsWith("Crm.", StringComparison.Ordinal)
        || permission.StartsWith("Reporting.", StringComparison.Ordinal);

    private static bool IsHrPermission(string permission) =>
        permission.StartsWith("HR.", StringComparison.Ordinal)
        || permission.StartsWith("Workflow.", StringComparison.Ordinal);
}
