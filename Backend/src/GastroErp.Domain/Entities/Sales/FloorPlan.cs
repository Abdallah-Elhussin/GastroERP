using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Sales;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>FloorPlan — خطة الأرضية (Aggregate Root)</summary>
public sealed class FloorPlan : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<DiningArea> _diningAreas = [];
    public IReadOnlyCollection<DiningArea> DiningAreas => _diningAreas.AsReadOnly();

    private FloorPlan() { NameAr = string.Empty; }

    public static FloorPlan Create(Guid tenantId, Guid branchId, string nameAr, string? nameEn = null, int sortOrder = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        return new FloorPlan
        {
            TenantId = tenantId,
            BranchId = branchId,
            NameAr = nameAr,
            NameEn = nameEn,
            IsActive = true,
            SortOrder = sortOrder
        };
    }

    public DiningArea AddArea(string nameAr, string? nameEn, int capacity, int sortOrder = 0)
    {
        var area = new DiningArea(Id, nameAr, nameEn, capacity, sortOrder);
        _diningAreas.Add(area);
        return area;
    }

    public void Update(string nameAr, string? nameEn, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        SortOrder = sortOrder;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public sealed class DiningArea : AuditableBaseEntity
{
    public Guid FloorPlanId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public int Capacity { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<RestaurantTable> _tables = [];
    public IReadOnlyCollection<RestaurantTable> Tables => _tables.AsReadOnly();

    private DiningArea() { NameAr = string.Empty; }

    internal DiningArea(Guid floorPlanId, string nameAr, string? nameEn, int capacity, int sortOrder)
    {
        FloorPlanId = floorPlanId;
        NameAr = nameAr;
        NameEn = nameEn;
        Capacity = capacity;
        SortOrder = sortOrder;
    }

    public RestaurantTable AddTable(string tableNumber, int capacity, TableShape shape,
        string? nameAr = null, string? nameEn = null, int? positionX = null, int? positionY = null)
    {
        var table = new RestaurantTable(Id, tableNumber, capacity, shape, nameAr, nameEn, positionX, positionY);
        _tables.Add(table);
        return table;
    }
}

public sealed class RestaurantTable : AuditableBaseEntity
{
    public Guid DiningAreaId { get; private set; }
    public string TableNumber { get; private set; }
    public string? NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public int Capacity { get; private set; }
    public TableStatus Status { get; private set; }
    public Guid? CurrentOrderId { get; private set; }
    public int? PositionX { get; private set; }
    public int? PositionY { get; private set; }
    public TableShape Shape { get; private set; }

    private RestaurantTable() { TableNumber = string.Empty; }

    internal RestaurantTable(Guid diningAreaId, string tableNumber, int capacity, TableShape shape,
        string? nameAr, string? nameEn, int? positionX, int? positionY)
    {
        DiningAreaId = diningAreaId;
        TableNumber = tableNumber;
        NameAr = nameAr;
        NameEn = nameEn;
        Capacity = capacity;
        Shape = shape;
        PositionX = positionX;
        PositionY = positionY;
        Status = TableStatus.Available;
    }

    public void Occupy(Guid orderId)
    {
        if (Status is TableStatus.Occupied or TableStatus.OutOfService)
            throw new BusinessException(ErrorCodes.TableNotAvailable);

        var previous = Status;
        Status = TableStatus.Occupied;
        CurrentOrderId = orderId;
        RaiseDomainEvent(new TableStatusChangedEvent(Id, previous, Status, orderId));
    }

    public void Release()
    {
        if (Status != TableStatus.Occupied) return;
        var previous = Status;
        Status = TableStatus.Available;
        CurrentOrderId = null;
        RaiseDomainEvent(new TableStatusChangedEvent(Id, previous, Status, null));
    }

    public void SetStatus(TableStatus status)
    {
        if (status == TableStatus.Occupied && CurrentOrderId is null)
            throw new BusinessException(ErrorCodes.TableNotAvailable);

        var previous = Status;
        Status = status;
        if (status != TableStatus.Occupied) CurrentOrderId = null;
        RaiseDomainEvent(new TableStatusChangedEvent(Id, previous, status, CurrentOrderId));
    }

    public void Reserve()
    {
        if (Status != TableStatus.Available)
            throw new BusinessException(ErrorCodes.TableNotAvailable);
        var previous = Status;
        Status = TableStatus.Reserved;
        RaiseDomainEvent(new TableStatusChangedEvent(Id, previous, Status, null));
    }

    public void Update(string tableNumber, int capacity, TableShape shape, string? nameAr, string? nameEn, int? positionX, int? positionY)
    {
        TableNumber = tableNumber;
        Capacity = capacity;
        Shape = shape;
        NameAr = nameAr;
        NameEn = nameEn;
        PositionX = positionX;
        PositionY = positionY;
    }
}
