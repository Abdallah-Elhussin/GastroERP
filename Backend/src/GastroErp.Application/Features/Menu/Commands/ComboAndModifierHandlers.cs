using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Domain.Entities.Menu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace GastroErp.Application.Features.Menu.Commands;

// ─── Combo Commands ───────────────────────────────────────────────────────────

public class CreateComboCommandHandler : IRequestHandler<CreateComboCommand, Result<ComboDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateComboCommandHandler> _logger;

    public CreateComboCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateComboCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<ComboDto>> Handle(CreateComboCommand request, CancellationToken cancellationToken)
    {
        var combo = new ComboMeal(
            request.TenantId,
            request.Dto.NameAr,
            request.Dto.ComboPrice,
            request.Dto.Currency ?? "SAR",
            request.Dto.NameEn,
            request.Dto.StartDate,
            request.Dto.EndDate
        );

        _context.ComboMeals.Add(combo);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Combo created: {Id}", combo.Id);
        return Result<ComboDto>.Success(_mapper.Map<ComboDto>(combo));
    }
}

public class UpdateComboCommandHandler : IRequestHandler<UpdateComboCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateComboCommandHandler> _logger;

    public UpdateComboCommandHandler(IApplicationDbContext context, ILogger<UpdateComboCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateComboCommand request, CancellationToken cancellationToken)
    {
        var combo = await _context.ComboMeals.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (combo == null) return Result.Failure("ComboNotFound", "Combo not found.");

        // ComboMeal doesn't have a direct method to update all fields. 
        // We can update the price.
        combo.UpdatePrice(request.Dto.ComboPrice);

        // Note: NameAr and other fields cannot be updated directly as ComboMeal doesn't expose a method for it.
        // If we strictly follow DDD, we only update what's exposed.

        _context.ComboMeals.Update(combo);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Combo updated: {Id}", combo.Id);
        return Result.Success();
    }
}

// ─── Modifier Commands ────────────────────────────────────────────────────────

public class CreateModifierCommandHandler : IRequestHandler<CreateModifierCommand, Result<ModifierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateModifierCommandHandler> _logger;

    public CreateModifierCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateModifierCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<ModifierDto>> Handle(CreateModifierCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.ModifierGroups.Include(mg => mg.Modifiers).FirstOrDefaultAsync(mg => mg.Id == request.Dto.ModifierGroupId, cancellationToken);
        if (group == null) return Result<ModifierDto>.Failure("ModifierGroupNotFound", "Modifier group not found.");

        group.AddModifier(request.Dto.NameAr, request.Dto.NameEn, request.Dto.ExtraPrice, request.Dto.IsDefault);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Find the recently added modifier to return
        var modifier = group.Modifiers.LastOrDefault();
        if (modifier != null)
        {
            _logger.LogInformation("Modifier created: {Id}", modifier.Id);
            return Result<ModifierDto>.Success(_mapper.Map<ModifierDto>(modifier));
        }

        return Result<ModifierDto>.Failure("FailedToCreateModifier", "Failed to create modifier.");
    }
}

public class UpdateModifierCommandHandler : IRequestHandler<UpdateModifierCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateModifierCommandHandler> _logger;

    public UpdateModifierCommandHandler(IApplicationDbContext context, ILogger<UpdateModifierCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateModifierCommand request, CancellationToken cancellationToken)
    {
        var modifier = await _context.Modifiers.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (modifier == null) return Result.Failure("ModifierNotFound", "Modifier not found.");

        modifier.UpdatePrice(request.Dto.ExtraPrice);

        _context.Modifiers.Update(modifier);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Modifier updated: {Id}", modifier.Id);
        return Result.Success();
    }
}

public class DeactivateComboCommandHandler : IRequestHandler<DeactivateComboCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateComboCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateComboCommand request, CancellationToken cancellationToken)
    {
        var combo = await _context.ComboMeals.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (combo == null) return Result.Failure("ComboNotFound", "Combo not found.");

        combo.Deactivate();
        _context.ComboMeals.Update(combo);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateComboCommandHandler : IRequestHandler<ActivateComboCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateComboCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateComboCommand request, CancellationToken cancellationToken)
    {
        var combo = await _context.ComboMeals.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (combo == null) return Result.Failure("ComboNotFound", "Combo not found.");

        combo.Activate();
        _context.ComboMeals.Update(combo);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class AddComboItemCommandHandler : IRequestHandler<AddComboItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddComboItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddComboItemCommand request, CancellationToken cancellationToken)
    {
        var combo = await _context.ComboMeals.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == request.ComboMealId, cancellationToken);
        if (combo == null) return Result.Failure("ComboNotFound", "Combo not found.");

        combo.AddItem(request.Dto.ProductId, request.Dto.Quantity, request.Dto.AllowSubstitution, request.Dto.SubstitutionCategoryId);
        _context.ComboMeals.Update(combo);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RemoveComboItemCommandHandler : IRequestHandler<RemoveComboItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveComboItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveComboItemCommand request, CancellationToken cancellationToken)
    {
        var combo = await _context.ComboMeals.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == request.ComboMealId, cancellationToken);
        if (combo == null) return Result.Failure("ComboNotFound", "Combo not found.");

        combo.RemoveItem(request.ProductId);
        _context.ComboMeals.Update(combo);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateComboItemCommandHandler : IRequestHandler<UpdateComboItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateComboItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateComboItemCommand request, CancellationToken cancellationToken)
    {
        var combo = await _context.ComboMeals.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == request.ComboMealId, cancellationToken);
        if (combo == null) return Result.Failure("ComboNotFound", "Combo not found.");

        var item = combo.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item == null) return Result.Failure("ComboItemNotFound", "Combo item not found.");

        item.UpdateQuantity(request.Dto.Quantity);
        _context.ComboMeals.Update(combo);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
