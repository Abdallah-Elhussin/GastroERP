# Phase 22: Finance & Accounting Integration Roadmap

## 1. Overview
The Finance & Accounting module is responsible for managing financial transactions, accounts, journal entries, ledgers, and reporting across the Gastro ERP system. It integrates heavily with Sales, Inventory, and CRM.

## 2. Core Entities
- `Account`: Chart of Accounts (Assets, Liabilities, Equity, Revenue, Expenses).
- `JournalEntry`: A double-entry accounting record.
- `JournalEntryLine`: Individual debit/credit lines.
- `FiscalYear` & `AccountingPeriod`: Timeframes for financial reporting.
- `TaxProfile` & `TaxTransaction`: Tax rules and collected/paid taxes.
- `Expense` & `ExpenseCategory`: Operational expenses outside of inventory.
- `BankTransaction` & `BankAccount`: Cash and bank management.

## 3. CQRS Commands
- `CreateAccountCommand`
- `CreateJournalEntryCommand` (System and Manual)
- `ApproveJournalEntryCommand`
- `OpenAccountingPeriodCommand`
- `CloseAccountingPeriodCommand`
- `RecordExpenseCommand`
- `ReconcileBankTransactionCommand`

## 4. CQRS Queries
- `GetAccountBalanceQuery`
- `GetLedgerReportQuery`
- `GetIncomeStatementQuery`
- `GetBalanceSheetQuery`
- `GetCashFlowStatementQuery`
- `GetTaxReportQuery`

## 5. Domain Events
- `JournalEntryPostedEvent`
- `AccountingPeriodClosedEvent`
- `ExpenseRecordedEvent`
- `BankTransactionReconciledEvent`

## 6. Integration Points
- **Sales**: End of Day (EOD) closures generate Revenue and Cash/Receivable journal entries.
- **Inventory**: Goods Receipts (GRPO) generate Inventory and Accounts Payable journal entries. Cost of Goods Sold (COGS) entries on sales.
- **CRM/Loyalty**: Redeeming points or gift cards generate liability reduction entries.

## 7. Implementation Steps
1. Define Enums (AccountType, EntryType, PeriodStatus).
2. Create Entities & Value Objects.
3. Configure EF Core mappings.
4. Implement CQRS (DTOs, Commands, Queries).
5. Build Application Services (LedgerService, TaxCalculationService).
6. Expose REST API Endpoints.
7. Write Unit Tests for double-entry logic to ensure credits always equal debits.
