using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GastroErp.Presentation.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole("Administrator"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var permissions = context.User.Claims
            .Where(x => x.Type is "Permission" or "permissions")
            .Select(x => x.Value)
            .ToHashSet(StringComparer.Ordinal);

        if (HasPermission(permissions, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool HasPermission(HashSet<string> permissions, string required)
    {
        if (permissions.Contains(required) || permissions.Contains("ALL"))
            return true;

        // Home / executive dashboard — allow any common operational view grant.
        if (required.Equals("Dashboard.View", StringComparison.Ordinal))
        {
            if (permissions.Contains("Reports.View")
                || permissions.Contains("Sales.Dashboard.View")
                || permissions.Contains("Sales.View")
                || permissions.Contains("Inventory.View")
                || permissions.Contains("Finance.View")
                || permissions.Contains("Accounting.View")
                || permissions.Contains("Hr.Dashboard.View"))
                return true;
        }

        // Legacy inventory grants imply finer ItemTypes permissions.
        if (required.StartsWith("Inventory.ItemTypes.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage"))
                return true;
            if (required.EndsWith(".View", StringComparison.Ordinal)
                && permissions.Contains("Inventory.View"))
                return true;
        }

        // Legacy inventory grants imply ValuationGroups permissions.
        if (required.StartsWith("Inventory.ValuationGroups.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage"))
                return true;
            if (required.EndsWith(".View", StringComparison.Ordinal)
                && permissions.Contains("Inventory.View"))
                return true;
            if ((required.EndsWith(".Create", StringComparison.Ordinal)
                 || required.EndsWith(".Edit", StringComparison.Ordinal)
                 || required.EndsWith(".Delete", StringComparison.Ordinal)
                 || required.EndsWith(".Export", StringComparison.Ordinal))
                && permissions.Contains("Inventory.Manage"))
                return true;
        }

        // Legacy inventory grants imply Settings permissions.
        if (required.StartsWith("Inventory.Settings.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage"))
                return true;
            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Inventory.View") || permissions.Contains("Inventory.Manage")))
                return true;
        }

        // Legacy Warehouse.* / Inventory.* grants imply Inventory.Warehouses.*
        if (required.StartsWith("Inventory.Warehouses.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage") || permissions.Contains("Warehouse.Update"))
                return true;
            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Inventory.View") || permissions.Contains("Warehouse.View")))
                return true;
            if (required.EndsWith(".Create", StringComparison.Ordinal) && permissions.Contains("Warehouse.Create"))
                return true;
            if (required.EndsWith(".Edit", StringComparison.Ordinal)
                && (permissions.Contains("Warehouse.Update") || permissions.Contains("Warehouse.Create")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Warehouse.Update"))
                return true;
            if (required.EndsWith(".Export", StringComparison.Ordinal)
                && (permissions.Contains("Inventory.View") || permissions.Contains("Warehouse.View")))
                return true;
        }

        // Sales product pricing — Inventory/Sales grants imply ProductPricing permissions.
        if (required.StartsWith("Sales.ProductPricing.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage") || permissions.Contains("Sales.Update") || permissions.Contains("Sales.Create"))
                return true;
            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Inventory.View") || permissions.Contains("Sales.View")))
                return true;
            if (required.EndsWith(".Copy", StringComparison.Ordinal)
                && (permissions.Contains("Inventory.Manage") || permissions.Contains("Sales.Create")))
                return true;
        }

        // Product inquiry — Inventory grants imply ProductInquiry permissions.
        if (required.StartsWith("Inventory.ProductInquiry.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal)
                 || required.EndsWith(".ViewCost", StringComparison.Ordinal)
                 || required.EndsWith(".ViewPrices", StringComparison.Ordinal)
                 || required.EndsWith(".ViewMovements", StringComparison.Ordinal)
                 || required.EndsWith(".ViewSuppliers", StringComparison.Ordinal)
                 || required.EndsWith(".Export", StringComparison.Ordinal))
                && permissions.Contains("Inventory.View"))
                return true;
        }

        // Issue destinations — Inventory grants imply IssueDestinations permissions.
        if (required.StartsWith("Inventory.IssueDestinations.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal)
                 || required.EndsWith(".Export", StringComparison.Ordinal))
                && permissions.Contains("Inventory.View"))
                return true;
        }

        // Account classifications — Accounting grants imply Classifications permissions.
        if (required.StartsWith("Accounting.Classifications.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
        }

        // Cost centers — Accounting/Finance grants imply CostCenter permissions.
        if (required.StartsWith("CostCenter.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View") || permissions.Contains("CostCenter.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
            if (required.EndsWith(".Activate", StringComparison.Ordinal) && permissions.Contains("CostCenter.Update"))
                return true;
            if (required.EndsWith(".Import", StringComparison.Ordinal) && permissions.Contains("CostCenter.Create"))
                return true;
        }

        // Currencies — Accounting/Finance grants imply Currency permissions.
        if (required.StartsWith("Currency.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View") || permissions.Contains("Currency.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
            if (required.EndsWith(".Activate", StringComparison.Ordinal) && permissions.Contains("Currency.Update"))
                return true;
            if (required.EndsWith(".SetCompany", StringComparison.Ordinal)
                && (permissions.Contains("Currency.SetCompany") || permissions.Contains("Accounting.Update")))
                return true;
            if (required.EndsWith(".ManageRates", StringComparison.Ordinal)
                && (permissions.Contains("Currency.ManageRates") || permissions.Contains("Currency.Update") || permissions.Contains("Accounting.Update")))
                return true;
            if (required.EndsWith(".Import", StringComparison.Ordinal) && permissions.Contains("Currency.Create"))
                return true;
        }

        // Document types — Accounting/Finance grants imply DocumentType permissions.
        if (required.StartsWith("DocumentType.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View") || permissions.Contains("DocumentType.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
            if (required.EndsWith(".Activate", StringComparison.Ordinal) && permissions.Contains("DocumentType.Update"))
                return true;
        }

        if (required.StartsWith("Bank.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View") || permissions.Contains("Bank.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
            if (required.EndsWith(".Activate", StringComparison.Ordinal) && permissions.Contains("Bank.Update"))
                return true;
        }

        if (required.StartsWith("CashBox.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal)
                 || required.EndsWith(".ViewMovements", StringComparison.Ordinal) || required.EndsWith(".PrintReport", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View") || permissions.Contains("CashBox.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
            if (required.EndsWith(".Activate", StringComparison.Ordinal) && permissions.Contains("CashBox.Update"))
                return true;
        }

        if (required.StartsWith("TaxRegistration.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal)
                 || required.EndsWith(".DownloadCertificate", StringComparison.Ordinal) || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View") || permissions.Contains("TaxRegistration.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
            if (required.EndsWith(".UploadCertificate", StringComparison.Ordinal)
                && (permissions.Contains("TaxRegistration.Update") || permissions.Contains("Accounting.Update")))
                return true;
        }

        // Tax code coding — Settings.TaxCodes.* + Accounting/Finance grants.
        if (required.StartsWith("Settings.TaxCodes.", StringComparison.Ordinal)
            || required.StartsWith("TaxCode.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create")
                || permissions.Contains("Tenant.Manage"))
                return true;

            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Settings.TaxCodes.View") || permissions.Contains("TaxCode.View")
                    || permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")))
                return true;

            if (required.EndsWith(".Create", StringComparison.Ordinal)
                && (permissions.Contains("Settings.TaxCodes.Create") || permissions.Contains("TaxCode.Create")))
                return true;

            if ((required.EndsWith(".Edit", StringComparison.Ordinal) || required.EndsWith(".Update", StringComparison.Ordinal))
                && (permissions.Contains("Settings.TaxCodes.Edit") || permissions.Contains("TaxCode.Update")
                    || permissions.Contains("Accounting.Update")))
                return true;

            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("Settings.TaxCodes.Delete") || permissions.Contains("TaxCode.Delete")
                    || permissions.Contains("Accounting.Delete")))
                return true;

            if ((required.EndsWith(".Export", StringComparison.Ordinal) || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Settings.TaxCodes.Export") || permissions.Contains("Settings.TaxCodes.Print")
                    || permissions.Contains("Settings.TaxCodes.View") || permissions.Contains("Accounting.View")
                    || permissions.Contains("Finance.View")))
                return true;
        }

        // Notification reason coding — Settings.NotificationReasons.*
        if (required.StartsWith("Settings.NotificationReasons.", StringComparison.Ordinal)
            || required.StartsWith("NotificationReason.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create")
                || permissions.Contains("Tenant.Manage"))
                return true;

            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Settings.NotificationReasons.View")
                    || permissions.Contains("NotificationReason.View")
                    || permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")))
                return true;

            if (required.EndsWith(".Create", StringComparison.Ordinal)
                && (permissions.Contains("Settings.NotificationReasons.Create")
                    || permissions.Contains("NotificationReason.Create")))
                return true;

            if ((required.EndsWith(".Edit", StringComparison.Ordinal) || required.EndsWith(".Update", StringComparison.Ordinal))
                && (permissions.Contains("Settings.NotificationReasons.Edit")
                    || permissions.Contains("NotificationReason.Update")
                    || permissions.Contains("Accounting.Update")))
                return true;

            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("Settings.NotificationReasons.Delete")
                    || permissions.Contains("NotificationReason.Delete")
                    || permissions.Contains("Accounting.Delete")))
                return true;

            if ((required.EndsWith(".Export", StringComparison.Ordinal) || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Settings.NotificationReasons.Export")
                    || permissions.Contains("Settings.NotificationReasons.Print")
                    || permissions.Contains("Settings.NotificationReasons.View")
                    || permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")))
                return true;
        }

        // Financial opening balances — Finance.OpeningBalances.*
        if (required.StartsWith("Finance.OpeningBalances.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create")
                || permissions.Contains("Tenant.Manage") || permissions.Contains("Journal.Create"))
                return true;

            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Finance.OpeningBalances.View")
                    || permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")
                    || permissions.Contains("Journal.View")))
                return true;

            if (required.EndsWith(".Create", StringComparison.Ordinal)
                && (permissions.Contains("Finance.OpeningBalances.Create") || permissions.Contains("Journal.Create")))
                return true;

            if ((required.EndsWith(".Edit", StringComparison.Ordinal) || required.EndsWith(".Update", StringComparison.Ordinal))
                && (permissions.Contains("Finance.OpeningBalances.Edit") || permissions.Contains("Accounting.Update")))
                return true;

            if (required.EndsWith(".Post", StringComparison.Ordinal)
                && (permissions.Contains("Finance.OpeningBalances.Post") || permissions.Contains("Journal.Post")))
                return true;

            if (required.EndsWith(".Reverse", StringComparison.Ordinal)
                && (permissions.Contains("Finance.OpeningBalances.Reverse") || permissions.Contains("Journal.Reverse")))
                return true;

            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("Finance.OpeningBalances.Delete") || permissions.Contains("Accounting.Delete")))
                return true;

            if ((required.EndsWith(".Export", StringComparison.Ordinal) || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Finance.OpeningBalances.Export")
                    || permissions.Contains("Finance.OpeningBalances.Print")
                    || permissions.Contains("Finance.OpeningBalances.View")
                    || permissions.Contains("Accounting.View")))
                return true;
        }

        // Receipt vouchers — Finance.ReceiptVouchers.*
        if (required.StartsWith("Finance.ReceiptVouchers.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create")
                || permissions.Contains("Tenant.Manage") || permissions.Contains("Journal.Create"))
                return true;

            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Finance.ReceiptVouchers.View")
                    || permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")
                    || permissions.Contains("Journal.View")))
                return true;

            if (required.EndsWith(".Create", StringComparison.Ordinal)
                && (permissions.Contains("Finance.ReceiptVouchers.Create") || permissions.Contains("Journal.Create")))
                return true;

            if ((required.EndsWith(".Edit", StringComparison.Ordinal) || required.EndsWith(".Update", StringComparison.Ordinal))
                && (permissions.Contains("Finance.ReceiptVouchers.Edit") || permissions.Contains("Accounting.Update")))
                return true;

            if (required.EndsWith(".Approve", StringComparison.Ordinal)
                && (permissions.Contains("Finance.ReceiptVouchers.Approve")
                    || permissions.Contains("Finance.ReceiptVouchers.Post")
                    || permissions.Contains("Journal.Post")))
                return true;

            if (required.EndsWith(".Post", StringComparison.Ordinal)
                && (permissions.Contains("Finance.ReceiptVouchers.Post") || permissions.Contains("Journal.Post")))
                return true;

            if (required.EndsWith(".Reverse", StringComparison.Ordinal)
                && (permissions.Contains("Finance.ReceiptVouchers.Reverse") || permissions.Contains("Journal.Reverse")))
                return true;

            if (required.EndsWith(".Cancel", StringComparison.Ordinal)
                && (permissions.Contains("Finance.ReceiptVouchers.Cancel")
                    || permissions.Contains("Finance.ReceiptVouchers.Edit")
                    || permissions.Contains("Accounting.Update")))
                return true;

            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("Finance.ReceiptVouchers.Delete") || permissions.Contains("Accounting.Delete")))
                return true;

            if ((required.EndsWith(".Export", StringComparison.Ordinal) || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Finance.ReceiptVouchers.Export")
                    || permissions.Contains("Finance.ReceiptVouchers.Print")
                    || permissions.Contains("Finance.ReceiptVouchers.View")
                    || permissions.Contains("Accounting.View")))
                return true;
        }

        // Financial notes — Finance.FinancialNotes.*
        if (required.StartsWith("Finance.FinancialNotes.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create")
                || permissions.Contains("Tenant.Manage") || permissions.Contains("Journal.Create"))
                return true;

            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Finance.FinancialNotes.View")
                    || permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")
                    || permissions.Contains("Journal.View")))
                return true;

            if (required.EndsWith(".Create", StringComparison.Ordinal)
                && (permissions.Contains("Finance.FinancialNotes.Create") || permissions.Contains("Journal.Create")))
                return true;

            if ((required.EndsWith(".Edit", StringComparison.Ordinal) || required.EndsWith(".Update", StringComparison.Ordinal))
                && (permissions.Contains("Finance.FinancialNotes.Edit") || permissions.Contains("Accounting.Update")))
                return true;

            if (required.EndsWith(".Approve", StringComparison.Ordinal)
                && (permissions.Contains("Finance.FinancialNotes.Approve")
                    || permissions.Contains("Finance.FinancialNotes.Post")
                    || permissions.Contains("Journal.Post")))
                return true;

            if (required.EndsWith(".Post", StringComparison.Ordinal)
                && (permissions.Contains("Finance.FinancialNotes.Post") || permissions.Contains("Journal.Post")))
                return true;

            if (required.EndsWith(".Reverse", StringComparison.Ordinal)
                && (permissions.Contains("Finance.FinancialNotes.Reverse") || permissions.Contains("Journal.Reverse")))
                return true;

            if (required.EndsWith(".Cancel", StringComparison.Ordinal)
                && (permissions.Contains("Finance.FinancialNotes.Cancel")
                    || permissions.Contains("Finance.FinancialNotes.Edit")
                    || permissions.Contains("Accounting.Update")))
                return true;

            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("Finance.FinancialNotes.Delete") || permissions.Contains("Accounting.Delete")))
                return true;
        }

        // Journal vouchers — Edit/Approve aliases for existing Journal.Create/Post grants.
        if (required.Equals("Journal.Edit", StringComparison.Ordinal)
            && (permissions.Contains("Journal.Edit")
                || permissions.Contains("Journal.Create")
                || permissions.Contains("Accounting.Update")
                || permissions.Contains("Accounting.Create")))
            return true;

        if (required.Equals("Journal.Approve", StringComparison.Ordinal)
            && (permissions.Contains("Journal.Approve")
                || permissions.Contains("Journal.Post")
                || permissions.Contains("Accounting.Update")))
            return true;

        if (required.StartsWith("Finance.GeneralLedgerSettings.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if ((required.EndsWith(".View", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal)
                 || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")
                    || permissions.Contains("Finance.GeneralLedgerSettings.View")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal) && permissions.Contains("Accounting.Delete"))
                return true;
            if (required.EndsWith(".Edit", StringComparison.Ordinal) && permissions.Contains("Accounting.Update"))
                return true;
        }

        // General Ledger inquiry — read-only ops screen.
        if (required.StartsWith("Finance.GeneralLedger.", StringComparison.Ordinal)
            && !required.StartsWith("Finance.GeneralLedgerSettings.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Reports.Accounting.View")
                || permissions.Contains("Accounting.View")
                || permissions.Contains("Finance.View")
                || permissions.Contains("Finance.GeneralLedger.View")
                || permissions.Contains("Reports.View"))
                return true;
        }

        // Branch coding — Settings.Branches.* aliases and Accounting/Tenant grants.
        if (required.StartsWith("Branch.", StringComparison.Ordinal)
            || required.StartsWith("Settings.Branches.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create")
                || permissions.Contains("Tenant.Manage") || permissions.Contains("Organization.Update"))
                return true;

            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Branch.View") || permissions.Contains("Settings.Branches.View")
                    || permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")))
                return true;

            if ((required.EndsWith(".Create", StringComparison.Ordinal))
                && (permissions.Contains("Branch.Create") || permissions.Contains("Settings.Branches.Create")))
                return true;

            if ((required.EndsWith(".Update", StringComparison.Ordinal) || required.EndsWith(".Edit", StringComparison.Ordinal)
                 || required.EndsWith(".Activate", StringComparison.Ordinal))
                && (permissions.Contains("Branch.Update") || permissions.Contains("Settings.Branches.Edit")
                    || permissions.Contains("Branch.Activate")))
                return true;

            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("Branch.Delete") || permissions.Contains("Settings.Branches.Delete")
                    || permissions.Contains("Accounting.Delete")))
                return true;

            if ((required.EndsWith(".Export", StringComparison.Ordinal) || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Branch.View") || permissions.Contains("Settings.Branches.View")
                    || permissions.Contains("Branch.Export") || permissions.Contains("Settings.Branches.Export")
                    || permissions.Contains("Branch.Print") || permissions.Contains("Settings.Branches.Print")))
                return true;
        }

        // User coding — Settings.Users.* / Identity.Users.*
        if (required.StartsWith("Identity.Users.", StringComparison.Ordinal)
            || required.StartsWith("Settings.Users.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Identity.Manage") || permissions.Contains("Identity.Users.Manage")
                || permissions.Contains("Tenant.Manage") || permissions.Contains("Settings.Manage"))
                return true;

            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Identity.Users.View") || permissions.Contains("Settings.Users.View")
                    || permissions.Contains("Identity.View") || permissions.Contains("Settings.View")))
                return true;

            if (required.EndsWith(".Create", StringComparison.Ordinal)
                && (permissions.Contains("Identity.Users.Create") || permissions.Contains("Settings.Users.Create")))
                return true;

            if ((required.EndsWith(".Edit", StringComparison.Ordinal) || required.EndsWith(".Update", StringComparison.Ordinal))
                && (permissions.Contains("Identity.Users.Edit") || permissions.Contains("Settings.Users.Edit")
                    || permissions.Contains("Identity.Users.Manage")))
                return true;

            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("Identity.Users.Delete") || permissions.Contains("Settings.Users.Delete")))
                return true;

            if (required.EndsWith(".ResetPassword", StringComparison.Ordinal)
                && (permissions.Contains("Identity.Users.ResetPassword") || permissions.Contains("Settings.Users.ResetPassword")
                    || permissions.Contains("Identity.Users.Manage")))
                return true;

            if (required.EndsWith(".LockUnlock", StringComparison.Ordinal)
                && (permissions.Contains("Identity.Users.LockUnlock") || permissions.Contains("Settings.Users.LockUnlock")
                    || permissions.Contains("Identity.Users.Manage")))
                return true;

            if ((required.EndsWith(".Export", StringComparison.Ordinal) || required.EndsWith(".Print", StringComparison.Ordinal))
                && (permissions.Contains("Identity.Users.View") || permissions.Contains("Settings.Users.View")
                    || permissions.Contains("Identity.Users.Export") || permissions.Contains("Settings.Users.Export")
                    || permissions.Contains("Identity.Users.Print") || permissions.Contains("Settings.Users.Print")))
                return true;
        }

        // Roles view also available to user managers (coding screen role combo).
        if (required.Equals("Identity.Roles.View", StringComparison.Ordinal))
        {
            if (permissions.Contains("Identity.Users.View")
                || permissions.Contains("Settings.Users.View")
                || permissions.Contains("Identity.Users.Manage")
                || permissions.Contains("Identity.Manage"))
                return true;
        }

        // Supplier master — Inventory grants imply Supplier permissions.
        if (required.StartsWith("Supplier.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Inventory.Manage") || permissions.Contains("Supplier.Manage"))
                return true;

            if ((required.EndsWith(".View", StringComparison.Ordinal)
                 || required.EndsWith(".ViewBalance", StringComparison.Ordinal)
                 || required.EndsWith(".ViewStatement", StringComparison.Ordinal))
                && (permissions.Contains("Inventory.View")
                    || permissions.Contains("Supplier.View")
                    || permissions.Contains("Purchase.View")))
                return true;

            if ((required.EndsWith(".Create", StringComparison.Ordinal)
                 || required.EndsWith(".Update", StringComparison.Ordinal)
                 || required.EndsWith(".Delete", StringComparison.Ordinal)
                 || required.EndsWith(".Activate", StringComparison.Ordinal)
                 || required.EndsWith(".Deactivate", StringComparison.Ordinal)
                 || required.EndsWith(".Blacklist", StringComparison.Ordinal)
                 || required.EndsWith(".ClearBlacklist", StringComparison.Ordinal)
                 || required.EndsWith(".UpdateFinancial", StringComparison.Ordinal)
                 || required.EndsWith(".ManageAttachments", StringComparison.Ordinal))
                && permissions.Contains("Inventory.Manage"))
                return true;
        }

        // Fiscal periods — Accounting/Finance grants imply FiscalPeriod permissions.
        if (required.StartsWith("FiscalPeriod.", StringComparison.Ordinal))
        {
            if (permissions.Contains("Accounting.Update") || permissions.Contains("Accounting.Create"))
                return true;
            if (required.EndsWith(".View", StringComparison.Ordinal)
                && (permissions.Contains("Accounting.View") || permissions.Contains("Finance.View")
                    || permissions.Contains("FiscalPeriod.View")))
                return true;
            if (required.EndsWith(".Create", StringComparison.Ordinal)
                && (permissions.Contains("FiscalPeriod.Create") || permissions.Contains("Accounting.Create")))
                return true;
            if ((required.EndsWith(".Edit", StringComparison.Ordinal) || required.EndsWith(".Export", StringComparison.Ordinal))
                && (permissions.Contains("FiscalPeriod.Edit") || permissions.Contains("Accounting.Update")
                    || permissions.Contains("FiscalPeriod.Create")))
                return true;
            if (required.EndsWith(".Delete", StringComparison.Ordinal)
                && (permissions.Contains("FiscalPeriod.Delete") || permissions.Contains("Accounting.Delete")))
                return true;
            if ((required.EndsWith(".Close", StringComparison.Ordinal)
                 || required.EndsWith(".Lock", StringComparison.Ordinal)
                 || required.EndsWith(".Reopen", StringComparison.Ordinal))
                && (permissions.Contains("Accounting.Update") || permissions.Contains("FiscalPeriod.Close")
                    || permissions.Contains("FiscalPeriod.Lock") || permissions.Contains("FiscalPeriod.Reopen")))
                return true;
        }

        return false;
    }
}
