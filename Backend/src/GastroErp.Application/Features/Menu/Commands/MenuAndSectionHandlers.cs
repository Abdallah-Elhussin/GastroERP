using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Domain.Entities.Menu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Menu.Commands;

// ─── Menu Handlers ────────────────────────────────────────────────────────────

public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Result<MenuDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateMenuCommandHandler> _logger;

    public CreateMenuCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateMenuCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<MenuDto>> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = new global::GastroErp.Domain.Entities.Menu.Menu(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.MenuType,
            request.Dto.SalesChannel,
            request.Dto.NameEn,
            request.Dto.StartDate,
            request.Dto.EndDate
        );

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Menu created: {MenuId}", menu.Id);
        return Result<MenuDto>.Success(_mapper.Map<MenuDto>(menu));
    }
}

public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateMenuCommandHandler> _logger;

    public UpdateMenuCommandHandler(IApplicationDbContext context, ILogger<UpdateMenuCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (menu == null) return Result.Failure("MenuNotFound", "Menu not found.");

        menu.UpdateDates(request.Dto.StartDate, request.Dto.EndDate);
        _context.Menus.Update(menu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Menu updated: {MenuId}", menu.Id);
        return Result.Success();
    }
}

public class ActivateMenuCommandHandler : IRequestHandler<ActivateMenuCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ActivateMenuCommandHandler> _logger;

    public ActivateMenuCommandHandler(IApplicationDbContext context, ILogger<ActivateMenuCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(ActivateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (menu == null) return Result.Failure("MenuNotFound", "Menu not found.");
        menu.Activate();
        _context.Menus.Update(menu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Menu activated: {MenuId}", menu.Id);
        return Result.Success();
    }
}

public class DeactivateMenuCommandHandler : IRequestHandler<DeactivateMenuCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateMenuCommandHandler> _logger;

    public DeactivateMenuCommandHandler(IApplicationDbContext context, ILogger<DeactivateMenuCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(DeactivateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (menu == null) return Result.Failure("MenuNotFound", "Menu not found.");
        menu.Deactivate();
        _context.Menus.Update(menu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Menu deactivated: {MenuId}", menu.Id);
        return Result.Success();
    }
}

// ─── MenuSection Handlers ─────────────────────────────────────────────────────

public class AddMenuSectionCommandHandler : IRequestHandler<AddMenuSectionCommand, Result<MenuSectionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AddMenuSectionCommandHandler> _logger;

    public AddMenuSectionCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<AddMenuSectionCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<MenuSectionDto>> Handle(AddMenuSectionCommand request, CancellationToken cancellationToken)
    {
        var menu = await _context.Menus
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == request.MenuId, cancellationToken);
        if (menu == null) return Result<MenuSectionDto>.Failure("MenuNotFound", "Menu not found.");

        var section = menu.AddSection(request.Dto.NameAr, request.Dto.NameEn, request.Dto.SortOrder);
        _context.Menus.Update(menu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("MenuSection added to Menu {MenuId}: {SectionId}", menu.Id, section.Id);
        return Result<MenuSectionDto>.Success(_mapper.Map<MenuSectionDto>(section));
    }
}

public class UpdateMenuSectionCommandHandler : IRequestHandler<UpdateMenuSectionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateMenuSectionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateMenuSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.MenuSections.FirstOrDefaultAsync(s => s.Id == request.SectionId && s.MenuId == request.MenuId, cancellationToken);
        if (section == null) return Result.Failure("SectionNotFound", "Menu section not found.");

        section.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DescriptionAr, request.Dto.DescriptionEn);
        _context.MenuSections.Update(section);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RemoveMenuSectionCommandHandler : IRequestHandler<RemoveMenuSectionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RemoveMenuSectionCommandHandler> _logger;

    public RemoveMenuSectionCommandHandler(IApplicationDbContext context, ILogger<RemoveMenuSectionCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(RemoveMenuSectionCommand request, CancellationToken cancellationToken)
    {
        var menu = await _context.Menus
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == request.MenuId, cancellationToken);
        if (menu == null) return Result.Failure("MenuNotFound", "Menu not found.");

        menu.RemoveSection(request.SectionId);
        _context.Menus.Update(menu);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("MenuSection {SectionId} removed from Menu {MenuId}", request.SectionId, request.MenuId);
        return Result.Success();
    }
}

public class DeactivateMenuSectionCommandHandler : IRequestHandler<DeactivateMenuSectionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateMenuSectionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateMenuSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.MenuSections.FirstOrDefaultAsync(s => s.Id == request.SectionId, cancellationToken);
        if (section == null) return Result.Failure("SectionNotFound", "Menu section not found.");
        section.Deactivate();
        _context.MenuSections.Update(section);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
