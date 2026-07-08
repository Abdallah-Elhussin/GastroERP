using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Domain.Entities.Menu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Menu.Commands;

// ─── MenuItem Handlers ────────────────────────────────────────────────────────

public class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddMenuItemCommandHandler> _logger;

    public AddMenuItemCommandHandler(IApplicationDbContext context, ILogger<AddMenuItemCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.MenuSections
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.SectionId, cancellationToken);
        if (section == null) return Result.Failure("SectionNotFound", "Menu section not found.");

        section.AddItem(request.Dto.ProductId, request.Dto.OverridePrice, request.Dto.SortOrder);
        _context.MenuSections.Update(section);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("MenuItem added to Section {SectionId}: Product {ProductId}", request.SectionId, request.Dto.ProductId);
        return Result.Success();
    }
}

public class RemoveMenuItemCommandHandler : IRequestHandler<RemoveMenuItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveMenuItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveMenuItemCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.MenuSections
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.SectionId, cancellationToken);
        if (section == null) return Result.Failure("SectionNotFound", "Menu section not found.");

        section.RemoveItem(request.ProductId);
        _context.MenuSections.Update(section);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetMenuItemOverridePriceCommandHandler : IRequestHandler<SetMenuItemOverridePriceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetMenuItemOverridePriceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetMenuItemOverridePriceCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == request.MenuItemId, cancellationToken);
        if (item == null) return Result.Failure("MenuItemNotFound", "Menu item not found.");
        item.SetOverridePrice(request.Price);
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class MarkMenuItemOutOfStockCommandHandler : IRequestHandler<MarkMenuItemOutOfStockCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MarkMenuItemOutOfStockCommandHandler> _logger;

    public MarkMenuItemOutOfStockCommandHandler(IApplicationDbContext context, ILogger<MarkMenuItemOutOfStockCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(MarkMenuItemOutOfStockCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == request.MenuItemId, cancellationToken);
        if (item == null) return Result.Failure("MenuItemNotFound", "Menu item not found.");
        item.MarkOutOfStock();
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("MenuItem marked out of stock: {MenuItemId}", item.Id);
        return Result.Success();
    }
}

public class MarkMenuItemInStockCommandHandler : IRequestHandler<MarkMenuItemInStockCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkMenuItemInStockCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(MarkMenuItemInStockCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == request.MenuItemId, cancellationToken);
        if (item == null) return Result.Failure("MenuItemNotFound", "Menu item not found.");
        item.MarkInStock();
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class HideMenuItemCommandHandler : IRequestHandler<HideMenuItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public HideMenuItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(HideMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == request.MenuItemId, cancellationToken);
        if (item == null) return Result.Failure("MenuItemNotFound", "Menu item not found.");
        item.Hide();
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ShowMenuItemCommandHandler : IRequestHandler<ShowMenuItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ShowMenuItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ShowMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == request.MenuItemId, cancellationToken);
        if (item == null) return Result.Failure("MenuItemNotFound", "Menu item not found.");
        item.Show();
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── BranchMenu Handlers ──────────────────────────────────────────────────────

public class CreateBranchMenuCommandHandler : IRequestHandler<CreateBranchMenuCommand, Result<BranchMenuDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBranchMenuCommandHandler> _logger;

    public CreateBranchMenuCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateBranchMenuCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<BranchMenuDto>> Handle(CreateBranchMenuCommand request, CancellationToken cancellationToken)
    {
        var branchMenu = new BranchMenu(
            request.Dto.TenantId,
            request.Dto.BranchId,
            request.Dto.MenuId,
            request.Dto.PriceLevelId
        );

        _context.BranchMenus.Add(branchMenu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BranchMenu created: Branch {BranchId}, Menu {MenuId}", request.Dto.BranchId, request.Dto.MenuId);
        return Result<BranchMenuDto>.Success(_mapper.Map<BranchMenuDto>(branchMenu));
    }
}

public class SetBranchMenuPriceLevelCommandHandler : IRequestHandler<SetBranchMenuPriceLevelCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetBranchMenuPriceLevelCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetBranchMenuPriceLevelCommand request, CancellationToken cancellationToken)
    {
        var branchMenu = await _context.BranchMenus.FirstOrDefaultAsync(b => b.Id == request.BranchMenuId, cancellationToken);
        if (branchMenu == null) return Result.Failure("BranchMenuNotFound", "Branch menu not found.");
        branchMenu.SetPriceLevel(request.PriceLevelId);
        _context.BranchMenus.Update(branchMenu);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateBranchMenuCommandHandler : IRequestHandler<ActivateBranchMenuCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateBranchMenuCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateBranchMenuCommand request, CancellationToken cancellationToken)
    {
        var branchMenu = await _context.BranchMenus.FirstOrDefaultAsync(b => b.Id == request.BranchMenuId, cancellationToken);
        if (branchMenu == null) return Result.Failure("BranchMenuNotFound", "Branch menu not found.");
        branchMenu.Activate();
        _context.BranchMenus.Update(branchMenu);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateBranchMenuCommandHandler : IRequestHandler<DeactivateBranchMenuCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateBranchMenuCommandHandler> _logger;

    public DeactivateBranchMenuCommandHandler(IApplicationDbContext context, ILogger<DeactivateBranchMenuCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(DeactivateBranchMenuCommand request, CancellationToken cancellationToken)
    {
        var branchMenu = await _context.BranchMenus.FirstOrDefaultAsync(b => b.Id == request.BranchMenuId, cancellationToken);
        if (branchMenu == null) return Result.Failure("BranchMenuNotFound", "Branch menu not found.");
        branchMenu.Deactivate();
        _context.BranchMenus.Update(branchMenu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("BranchMenu deactivated: {BranchMenuId}", branchMenu.Id);
        return Result.Success();
    }
}

public class SetMenuAvailabilityCommandHandler : IRequestHandler<SetMenuAvailabilityCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetMenuAvailabilityCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetMenuAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var branchMenu = await _context.BranchMenus
            .Include(b => b.Availabilities)
            .FirstOrDefaultAsync(b => b.Id == request.BranchMenuId, cancellationToken);
        if (branchMenu == null) return Result.Failure("BranchMenuNotFound", "Branch menu not found.");

        branchMenu.SetAvailability(request.Dto.DayOfWeek, request.Dto.StartTime, request.Dto.EndTime);
        _context.BranchMenus.Update(branchMenu);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
