using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// نوع مستند وإعدادات سير العمل — المرجع المركزي لجميع مستندات النظام.
/// </summary>
public sealed class DocumentType : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public string? Description { get; private set; }
    public DocumentModule Module { get; private set; }

    // Numbering
    public string Prefix { get; private set; }
    public string? Suffix { get; private set; }
    public long StartingNumber { get; private set; }
    public long LastNumber { get; private set; }
    public byte NumberLength { get; private set; }
    public bool ResetYearly { get; private set; }
    public bool ResetMonthly { get; private set; }
    public bool NumberPerBranch { get; private set; }
    public bool NumberPerCompany { get; private set; }

    // Approval / Workflow / Posting
    public DocumentApprovalMode ApprovalMode { get; private set; }
    public bool RequiresApproval { get; private set; }
    public bool UsesWorkflow { get; private set; }
    public Guid? WorkflowDefinitionId { get; private set; }
    public DocumentPostingMode PostingMode { get; private set; }
    public bool AutoPost { get; private set; }
    public bool PostAfterApproval { get; private set; }

    // Impact flags
    public bool AffectsInventory { get; private set; }
    public bool AffectsCost { get; private set; }
    public bool AffectsAccounting { get; private set; }
    public bool AffectsCash { get; private set; }
    public bool AffectsCustomers { get; private set; }
    public bool AffectsSuppliers { get; private set; }
    public bool AffectsAssets { get; private set; }
    public bool AffectsPayroll { get; private set; }

    // Capability permissions (feature switches)
    public bool AllowCreate { get; private set; } = true;
    public bool AllowUpdate { get; private set; } = true;
    public bool AllowApprove { get; private set; } = true;
    public bool AllowPost { get; private set; } = true;
    public bool AllowCancel { get; private set; } = true;
    public bool AllowDelete { get; private set; }

    // Extra settings
    public bool AllowAttachments { get; private set; } = true;
    public bool AllowPrint { get; private set; } = true;
    public bool AllowEditAfterSave { get; private set; } = true;
    public bool AllowDeleteDocuments { get; private set; }
    public bool AllowCancelDocuments { get; private set; } = true;
    public bool AllowCopy { get; private set; } = true;
    public bool AllowReopen { get; private set; }
    public bool ShowInReports { get; private set; } = true;
    public bool ShowInDashboard { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private readonly List<DocumentTypeLifecycleStage> _lifecycleStages = [];
    public IReadOnlyCollection<DocumentTypeLifecycleStage> LifecycleStages => _lifecycleStages.AsReadOnly();

    private DocumentType()
    {
        Code = string.Empty;
        NameAr = string.Empty;
        NameEn = string.Empty;
        Prefix = string.Empty;
    }

    public static DocumentType Create(
        Guid tenantId,
        string code,
        string nameAr,
        string nameEn,
        DocumentModule module,
        string prefix,
        string? description = null,
        string? suffix = null,
        long startingNumber = 1,
        byte numberLength = 6,
        int sortOrder = 0,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameEn)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(prefix)) throw new BusinessException(ErrorCodes.RequiredField);
        if (numberLength is < 1 or > 12) throw new BusinessException(ErrorCodes.DocumentTypeNumberLengthInvalid);
        if (startingNumber < 0) throw new BusinessException(ErrorCodes.DocumentTypeNumberInvalid);

        var doc = new DocumentType
        {
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            NameAr = nameAr.Trim(),
            NameEn = nameEn.Trim(),
            Description = Normalize(description),
            Module = module,
            Prefix = prefix.Trim().ToUpperInvariant(),
            Suffix = Normalize(suffix),
            StartingNumber = startingNumber,
            LastNumber = Math.Max(0, startingNumber - 1),
            NumberLength = numberLength,
            SortOrder = sortOrder,
            IsSystem = isSystem,
            IsActive = true
        };

        doc.ApplyDefaultLifecycle();
        return doc;
    }

    public void UpdateBasic(string nameAr, string nameEn, string? description, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameEn)) throw new BusinessException(ErrorCodes.RequiredField);
        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();
        Description = Normalize(description);
        SortOrder = sortOrder;
    }

    public void UpdateIdentity(string code, DocumentModule module)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField);
        Code = code.Trim().ToUpperInvariant();
        Module = module;
    }

    public void UpdateNumbering(
        string prefix,
        string? suffix,
        long startingNumber,
        long lastNumber,
        byte numberLength,
        bool resetYearly,
        bool resetMonthly,
        bool numberPerBranch,
        bool numberPerCompany)
    {
        if (string.IsNullOrWhiteSpace(prefix)) throw new BusinessException(ErrorCodes.RequiredField);
        if (numberLength is < 1 or > 12) throw new BusinessException(ErrorCodes.DocumentTypeNumberLengthInvalid);
        if (startingNumber < 0 || lastNumber < 0) throw new BusinessException(ErrorCodes.DocumentTypeNumberInvalid);
        if (lastNumber < startingNumber - 1)
            throw new BusinessException(ErrorCodes.DocumentTypeNumberInvalid);

        Prefix = prefix.Trim().ToUpperInvariant();
        Suffix = Normalize(suffix);
        StartingNumber = startingNumber;
        LastNumber = lastNumber;
        NumberLength = numberLength;
        ResetYearly = resetYearly;
        ResetMonthly = resetMonthly;
        NumberPerBranch = numberPerBranch;
        NumberPerCompany = numberPerCompany;
    }

    public void UpdateApproval(
        DocumentApprovalMode mode,
        bool requiresApproval,
        bool usesWorkflow,
        Guid? workflowDefinitionId)
    {
        ApprovalMode = mode;
        RequiresApproval = requiresApproval || mode != DocumentApprovalMode.None;
        UsesWorkflow = usesWorkflow;
        WorkflowDefinitionId = usesWorkflow ? workflowDefinitionId : null;
    }

    public void UpdatePosting(DocumentPostingMode mode, bool autoPost, bool postAfterApproval)
    {
        PostingMode = mode;
        AutoPost = autoPost || mode == DocumentPostingMode.AutoPost;
        PostAfterApproval = postAfterApproval || mode == DocumentPostingMode.PostAfterApproval;
    }

    public void UpdateImpact(
        bool inventory, bool cost, bool accounting, bool cash,
        bool customers, bool suppliers, bool assets, bool payroll)
    {
        AffectsInventory = inventory;
        AffectsCost = cost;
        AffectsAccounting = accounting;
        AffectsCash = cash;
        AffectsCustomers = customers;
        AffectsSuppliers = suppliers;
        AffectsAssets = assets;
        AffectsPayroll = payroll;
    }

    public void UpdateCapabilities(
        bool create, bool update, bool approve, bool post, bool cancel, bool delete)
    {
        AllowCreate = create;
        AllowUpdate = update;
        AllowApprove = approve;
        AllowPost = post;
        AllowCancel = cancel;
        AllowDelete = delete;
    }

    public void UpdateExtras(
        bool attachments, bool print, bool editAfterSave, bool deleteDocs, bool cancelDocs,
        bool copy, bool reopen, bool reports, bool dashboard)
    {
        AllowAttachments = attachments;
        AllowPrint = print;
        AllowEditAfterSave = editAfterSave;
        AllowDeleteDocuments = deleteDocs;
        AllowCancelDocuments = cancelDocs;
        AllowCopy = copy;
        AllowReopen = reopen;
        ShowInReports = reports;
        ShowInDashboard = dashboard;
    }

    public void ReplaceLifecycleStages(IEnumerable<(string Code, string NameAr, string NameEn, int SortOrder, bool IsTerminal)> stages)
    {
        _lifecycleStages.Clear();
        var list = stages.ToList();
        if (list.Count == 0)
        {
            ApplyDefaultLifecycle();
            return;
        }

        var order = 0;
        foreach (var s in list)
        {
            if (string.IsNullOrWhiteSpace(s.Code) || string.IsNullOrWhiteSpace(s.NameAr))
                throw new BusinessException(ErrorCodes.RequiredField);
            _lifecycleStages.Add(DocumentTypeLifecycleStage.Create(
                Id, s.Code, s.NameAr, s.NameEn, s.SortOrder == 0 ? order : s.SortOrder, s.IsTerminal));
            order++;
        }
    }

    public string FormatDocumentNumber(long number)
    {
        var body = number.ToString().PadLeft(NumberLength, '0');
        return string.IsNullOrWhiteSpace(Suffix)
            ? $"{Prefix}-{body}"
            : $"{Prefix}-{body}-{Suffix}";
    }

    public string AllocateNextNumber()
    {
        LastNumber++;
        if (LastNumber < StartingNumber)
            LastNumber = StartingNumber;
        return FormatDocumentNumber(LastNumber);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void EnsureCanDelete()
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.DocumentTypeProtected);
    }

    public void EnsureCanChangeIdentity()
    {
        // callers check usage; domain blocks system module/code lock is application-side after use
    }

    public DocumentType CloneAs(string newCode, string nameAr, string nameEn, string prefix)
    {
        var clone = Create(TenantId, newCode, nameAr, nameEn, Module, prefix, Description, Suffix,
            StartingNumber, NumberLength, SortOrder, isSystem: false);
        clone.UpdateNumbering(prefix, Suffix, StartingNumber, StartingNumber - 1, NumberLength,
            ResetYearly, ResetMonthly, NumberPerBranch, NumberPerCompany);
        clone.UpdateApproval(ApprovalMode, RequiresApproval, UsesWorkflow, WorkflowDefinitionId);
        clone.UpdatePosting(PostingMode, AutoPost, PostAfterApproval);
        clone.UpdateImpact(AffectsInventory, AffectsCost, AffectsAccounting, AffectsCash,
            AffectsCustomers, AffectsSuppliers, AffectsAssets, AffectsPayroll);
        clone.UpdateCapabilities(AllowCreate, AllowUpdate, AllowApprove, AllowPost, AllowCancel, AllowDelete);
        clone.UpdateExtras(AllowAttachments, AllowPrint, AllowEditAfterSave, AllowDeleteDocuments,
            AllowCancelDocuments, AllowCopy, AllowReopen, ShowInReports, ShowInDashboard);
        clone.ReplaceLifecycleStages(_lifecycleStages.Select(s =>
            (s.Code, s.NameAr, s.NameEn, s.SortOrder, s.IsTerminal)));
        return clone;
    }

    private void ApplyDefaultLifecycle()
    {
        _lifecycleStages.Clear();
        _lifecycleStages.Add(DocumentTypeLifecycleStage.Create(Id, "Draft", "مسودة", "Draft", 0, false));
        _lifecycleStages.Add(DocumentTypeLifecycleStage.Create(Id, "Submitted", "مقدم", "Submitted", 1, false));
        _lifecycleStages.Add(DocumentTypeLifecycleStage.Create(Id, "Approved", "معتمد", "Approved", 2, false));
        _lifecycleStages.Add(DocumentTypeLifecycleStage.Create(Id, "Posted", "مرحّل", "Posted", 3, false));
        _lifecycleStages.Add(DocumentTypeLifecycleStage.Create(Id, "Closed", "مغلق", "Closed", 4, true));
        _lifecycleStages.Add(DocumentTypeLifecycleStage.Create(Id, "Cancelled", "ملغى", "Cancelled", 5, true));
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>مرحلة ضمن دورة حياة نوع المستند.</summary>
public sealed class DocumentTypeLifecycleStage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentTypeId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsTerminal { get; private set; }

    private DocumentTypeLifecycleStage() { }

    public static DocumentTypeLifecycleStage Create(
        Guid documentTypeId, string code, string nameAr, string nameEn, int sortOrder, bool isTerminal)
        => new()
        {
            DocumentTypeId = documentTypeId,
            Code = code.Trim(),
            NameAr = nameAr.Trim(),
            NameEn = string.IsNullOrWhiteSpace(nameEn) ? code.Trim() : nameEn.Trim(),
            SortOrder = sortOrder,
            IsTerminal = isTerminal
        };
}
