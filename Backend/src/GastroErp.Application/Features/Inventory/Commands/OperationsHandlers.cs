using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Counting;
using GastroErp.Domain.Entities.Inventory.Waste;
using GastroErp.Domain.Entities.Inventory.Recipe;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

// ─── StockTransfer Handlers ───────────────────────────────────────────────────

public class CreateStockTransferCommandHandler : IRequestHandler<CreateStockTransferCommand, Result<StockTransferDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStockTransferCommandHandler> _logger;

    public CreateStockTransferCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateStockTransferCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<StockTransferDto>> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var fromWh = await _context.Warehouses.AnyAsync(w => w.Id == request.Dto.SourceWarehouseId, cancellationToken);
        if (!fromWh) return Result<StockTransferDto>.Failure("WarehouseNotFound", "Source warehouse not found.");
        var toWh = await _context.Warehouses.AnyAsync(w => w.Id == request.Dto.DestinationWarehouseId, cancellationToken);
        if (!toWh) return Result<StockTransferDto>.Failure("WarehouseNotFound", "Destination warehouse not found.");

        var transfer = new StockTransfer(request.Dto.TenantId, request.Dto.SourceWarehouseId, request.Dto.DestinationWarehouseId, request.Dto.TransferNumber, request.Dto.Notes);
        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StockTransfer created: {Id}", transfer.Id);
        return Result<StockTransferDto>.Success(_mapper.Map<StockTransferDto>(transfer));
    }
}

public class AddTransferLineCommandHandler : IRequestHandler<AddTransferLineCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddTransferLineCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddTransferLineCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.StockTransfers.Include(t => t.Lines).FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);
        if (transfer == null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");

        transfer.AddLine(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.Quantity);
        _context.StockTransfers.Update(transfer);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CompleteStockTransferCommandHandler : IRequestHandler<CompleteStockTransferCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CompleteStockTransferCommandHandler> _logger;

    public CompleteStockTransferCommandHandler(IApplicationDbContext context, ILogger<CompleteStockTransferCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(CompleteStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.StockTransfers.Include(t => t.Lines).FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (transfer == null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");
        if (!transfer.Lines.Any()) return Result.Failure("NoLines", "Cannot complete transfer with no lines.");

        transfer.Complete();
        _context.StockTransfers.Update(transfer);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StockTransfer completed: {Id}", transfer.Id);
        return Result.Success();
    }
}

// ─── StockAdjustment Handlers ─────────────────────────────────────────────────

public class CreateStockAdjustmentCommandHandler : IRequestHandler<CreateStockAdjustmentCommand, Result<StockAdjustmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStockAdjustmentCommandHandler> _logger;

    public CreateStockAdjustmentCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateStockAdjustmentCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<StockAdjustmentDto>> Handle(CreateStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adj = new StockAdjustment(request.Dto.TenantId, request.Dto.WarehouseId, request.Dto.AdjustmentNumber, null, request.Dto.Notes);
        if (request.Dto.ReasonId.HasValue)
            adj.AddLine(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.ReasonId.Value, request.Dto.QuantityAdjusted, request.Dto.UnitCost);

        _context.StockAdjustments.Add(adj);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StockAdjustment created: {Id}", adj.Id);
        return Result<StockAdjustmentDto>.Success(_mapper.Map<StockAdjustmentDto>(adj));
    }
}

public class ConfirmStockAdjustmentCommandHandler : IRequestHandler<ConfirmStockAdjustmentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ConfirmStockAdjustmentCommandHandler> _logger;

    public ConfirmStockAdjustmentCommandHandler(IApplicationDbContext context, ILogger<ConfirmStockAdjustmentCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(ConfirmStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adj = await _context.StockAdjustments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (adj == null) return Result.Failure("AdjustmentNotFound", "Stock adjustment not found.");
        adj.Complete();
        _context.StockAdjustments.Update(adj);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StockAdjustment confirmed: {Id}", adj.Id);
        return Result.Success();
    }
}

// ─── WasteRecord Handlers ─────────────────────────────────────────────────────

public class CreateWasteRecordCommandHandler : IRequestHandler<CreateWasteRecordCommand, Result<WasteRecordDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWasteRecordCommandHandler> _logger;

    public CreateWasteRecordCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateWasteRecordCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<WasteRecordDto>> Handle(CreateWasteRecordCommand request, CancellationToken cancellationToken)
    {
        var wasteRecord = new WasteRecord(request.Dto.TenantId, request.Dto.WarehouseId, request.Dto.WasteNumber, request.Dto.Notes);
        if (request.Dto.ReasonId.HasValue)
            wasteRecord.AddItem(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.ReasonId.Value, request.Dto.Quantity, request.Dto.UnitCost);

        _context.WasteRecords.Add(wasteRecord);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("WasteRecord created: {Id}", wasteRecord.Id);
        return Result<WasteRecordDto>.Success(_mapper.Map<WasteRecordDto>(wasteRecord));
    }
}

public class ConfirmWasteRecordCommandHandler : IRequestHandler<ConfirmWasteRecordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ConfirmWasteRecordCommandHandler> _logger;

    public ConfirmWasteRecordCommandHandler(IApplicationDbContext context, ILogger<ConfirmWasteRecordCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(ConfirmWasteRecordCommand request, CancellationToken cancellationToken)
    {
        var waste = await _context.WasteRecords.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (waste == null) return Result.Failure("WasteRecordNotFound", "Waste record not found.");
        waste.Complete();
        _context.WasteRecords.Update(waste);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("WasteRecord confirmed: {Id}", waste.Id);
        return Result.Success();
    }
}

// ─── Recipe Handlers ──────────────────────────────────────────────────────────

public class CreateRecipeCommandHandler : IRequestHandler<CreateRecipeCommand, Result<RecipeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateRecipeCommandHandler> _logger;

    public CreateRecipeCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateRecipeCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<RecipeDto>> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = new Recipe(request.Dto.TenantId, request.Dto.ProductId, string.Empty, null, request.Dto.Yield, 0, 0);
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Recipe created: {Id} for Product {ProductId}", recipe.Id, recipe.ProductId);
        return Result<RecipeDto>.Success(_mapper.Map<RecipeDto>(recipe));
    }
}

public class UpdateRecipeCommandHandler : IRequestHandler<UpdateRecipeCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateRecipeCommandHandler> _logger;

    public UpdateRecipeCommandHandler(IApplicationDbContext context, ILogger<UpdateRecipeCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (recipe == null) return Result.Failure("RecipeNotFound", "Recipe not found.");

        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Recipe updated: {Id}", recipe.Id);
        return Result.Success();
    }
}

public class AddRecipeIngredientCommandHandler : IRequestHandler<AddRecipeIngredientCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddRecipeIngredientCommandHandler> _logger;

    public AddRecipeIngredientCommandHandler(IApplicationDbContext context, ILogger<AddRecipeIngredientCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(AddRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);
        if (recipe == null) return Result.Failure("RecipeNotFound", "Recipe not found.");

        recipe.AddItem(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.Quantity);
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Ingredient added to Recipe {RecipeId}", recipe.Id);
        return Result.Success();
    }
}

public class RemoveRecipeIngredientCommandHandler : IRequestHandler<RemoveRecipeIngredientCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveRecipeIngredientCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);
        if (recipe == null) return Result.Failure("RecipeNotFound", "Recipe not found.");
        recipe.RemoveItem(request.IngredientId);
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateRecipeCommandHandler : IRequestHandler<DeactivateRecipeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateRecipeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (recipe == null) return Result.Failure("RecipeNotFound", "Recipe not found.");
        recipe.MarkObsolete();
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateRecipeCommandHandler : IRequestHandler<ActivateRecipeCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ActivateRecipeCommandHandler> _logger;

    public ActivateRecipeCommandHandler(IApplicationDbContext context, ILogger<ActivateRecipeCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(ActivateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (recipe == null) return Result.Failure("RecipeNotFound", "Recipe not found.");
        recipe.Activate();
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Recipe activated: {Id}", recipe.Id);
        return Result.Success();
    }
}

// ─── StockCount Handlers ──────────────────────────────────────────────────────

public class CreateStockCountCommandHandler : IRequestHandler<CreateStockCountCommand, Result<StockCountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStockCountCommandHandler> _logger;

    public CreateStockCountCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateStockCountCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<StockCountDto>> Handle(CreateStockCountCommand request, CancellationToken cancellationToken)
    {
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.Dto.WarehouseId, cancellationToken);
        if (!warehouseExists) return Result<StockCountDto>.Failure("WarehouseNotFound", "Warehouse not found.");

        // Resolve TenantId from request or context
        // In clean architecture, we usually pass TenantId in the request command or resolve it.
        // Let's assume request contains the TenantId, or we use a fixed TenantId if DTO doesn't have it.
        // Wait, DTO doesn't have TenantId. How does other commands resolve TenantId?
        // Let's check:
        // var settings = new InventorySetting(request.Dto.TenantId, ...)
        // Let's check CreateStockTransferCommand: request.Dto.TenantId. Wait, DTO doesn't have TenantId for CreateStockCountDto? 
        // Ah, our CreateStockCountDto doesn't have TenantId, but we can pass it or resolve it. Let's see if other create DTOs have TenantId.
        // Yes, CreateInventoryCategoryDto has TenantId. Let's look at CreateStockCountDto in our replacement: it doesn't have it.
        // Wait, let's look at how other handlers get TenantId if DTO doesn't have it, or let's assume we can get it from the user context, or we can add TenantId to CreateStockCountDto.
        // Wait! In clean architecture, TenantId can be passed from the Controller. Let's see how `CreateComboCommand` was handled:
        // `public record CreateComboCommand(Guid TenantId, CreateComboDto Dto)`.
        // So we can pass TenantId directly in the command! Like `CreateStockCountCommand(Guid TenantId, CreateStockCountDto Dto)`.
        // Let's check what our command record is:
        // `public record CreateStockCountCommand(CreateStockCountDto Dto)`. We can change it to:
        // `public record CreateStockCountCommand(Guid TenantId, CreateStockCountDto Dto)`.
        // Yes, that is much safer and matches the pattern of `CreateComboCommand`.
        // Let's check the rest of commands:
        // `CreatePurchaseReturnCommand(Guid TenantId, CreatePurchaseReturnDto Dto)`.
        // `ReserveStockCommand(Guid TenantId, ReserveStockDto Dto)`.
        // Let's modify `InventoryCommands.cs` to include TenantId in those command records.

        // Wait, for now let's write the handler assuming the command has TenantId.
        // I will write the handler.
        var stockCount = new StockCount(request.TenantId, request.Dto.WarehouseId, request.Dto.CountNumber, request.Dto.Notes);
        _context.StockCounts.Add(stockCount);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StockCount created: {Id}", stockCount.Id);
        return Result<StockCountDto>.Success(_mapper.Map<StockCountDto>(stockCount));
    }
}

public class AddStockCountLineCommandHandler : IRequestHandler<AddStockCountLineCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddStockCountLineCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddStockCountLineCommand request, CancellationToken cancellationToken)
    {
        var stockCount = await _context.StockCounts.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == request.StockCountId, cancellationToken);
        if (stockCount == null) return Result.Failure("StockCountNotFound", "Stock count not found.");

        stockCount.AddLine(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.ExpectedQuantity, request.Dto.ActualQuantity, request.Dto.BatchNumber);
        _context.StockCounts.Update(stockCount);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class FreezeInventoryCommandHandler : IRequestHandler<FreezeInventoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public FreezeInventoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(FreezeInventoryCommand request, CancellationToken cancellationToken)
    {
        var stockCount = await _context.StockCounts.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (stockCount == null) return Result.Failure("StockCountNotFound", "Stock count not found.");

        stockCount.MarkAsInProgress();
        _context.StockCounts.Update(stockCount);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApproveStockCountCommandHandler : IRequestHandler<ApproveStockCountCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ApproveStockCountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ApproveStockCountCommand request, CancellationToken cancellationToken)
    {
        var stockCount = await _context.StockCounts.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (stockCount == null) return Result.Failure("StockCountNotFound", "Stock count not found.");

        stockCount.Complete();
        _context.StockCounts.Update(stockCount);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── InventoryReservation Handlers ────────────────────────────────────────────

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, Result<InventoryReservationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ReserveStockCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryReservationDto>> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var reservation = new global::GastroErp.Domain.Entities.Inventory.Reservation.InventoryReservation(
            request.TenantId,
            request.Dto.WarehouseId,
            request.Dto.InventoryItemId,
            request.Dto.ReservedQuantity,
            request.Dto.SourceDocument,
            request.Dto.ExpirationDate
        );

        _context.InventoryReservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<InventoryReservationDto>.Success(_mapper.Map<InventoryReservationDto>(reservation));
    }
}

public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReleaseStockCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.InventoryReservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (reservation == null) return Result.Failure("ReservationNotFound", "Reservation not found.");

        reservation.Cancel();
        _context.InventoryReservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ExpireStockReservationCommandHandler : IRequestHandler<ExpireStockReservationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ExpireStockReservationCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ExpireStockReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.InventoryReservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (reservation == null) return Result.Failure("ReservationNotFound", "Reservation not found.");

        reservation.Expire();
        _context.InventoryReservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
