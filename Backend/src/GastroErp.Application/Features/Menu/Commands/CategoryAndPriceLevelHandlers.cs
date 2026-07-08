using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Domain.Entities.Menu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Menu.Commands;

// ─── Category Handlers ────────────────────────────────────────────────────────

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateCategoryCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.NameEn,
            request.Dto.ParentCategoryId,
            request.Dto.Color,
            request.Dto.Icon,
            request.Dto.SortOrder
        );

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category created: {CategoryId}", category.Id);
        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(IApplicationDbContext context, ILogger<UpdateCategoryCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null) return Result.Failure("CategoryNotFound", "Category not found.");

        category.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DescriptionAr, request.Dto.DescriptionEn, request.Dto.Color, request.Dto.Icon);
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category updated: {CategoryId}", category.Id);
        return Result.Success();
    }
}

public class SetCategoryImageCommandHandler : IRequestHandler<SetCategoryImageCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetCategoryImageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetCategoryImageCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null) return Result.Failure("CategoryNotFound", "Category not found.");
        category.SetImage(request.ImageUrl);
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateCategoryCommandHandler : IRequestHandler<DeactivateCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateCategoryCommandHandler> _logger;

    public DeactivateCategoryCommandHandler(IApplicationDbContext context, ILogger<DeactivateCategoryCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null) return Result.Failure("CategoryNotFound", "Category not found.");
        category.Deactivate();
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category deactivated: {CategoryId}", category.Id);
        return Result.Success();
    }
}

public class ActivateCategoryCommandHandler : IRequestHandler<ActivateCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null) return Result.Failure("CategoryNotFound", "Category not found.");
        category.Activate();
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── PriceLevel Handlers ──────────────────────────────────────────────────────

public class CreatePriceLevelCommandHandler : IRequestHandler<CreatePriceLevelCommand, Result<PriceLevelDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePriceLevelCommandHandler> _logger;

    public CreatePriceLevelCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreatePriceLevelCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<PriceLevelDto>> Handle(CreatePriceLevelCommand request, CancellationToken cancellationToken)
    {
        var priceLevel = new PriceLevel(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.SalesChannel,
            request.Dto.NameEn,
            request.Dto.IsDefault
        );

        _context.PriceLevels.Add(priceLevel);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("PriceLevel created: {PriceLevelId}", priceLevel.Id);
        return Result<PriceLevelDto>.Success(_mapper.Map<PriceLevelDto>(priceLevel));
    }
}

public class SetPriceLevelAsDefaultCommandHandler : IRequestHandler<SetPriceLevelAsDefaultCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetPriceLevelAsDefaultCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetPriceLevelAsDefaultCommand request, CancellationToken cancellationToken)
    {
        var priceLevel = await _context.PriceLevels.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (priceLevel == null) return Result.Failure("PriceLevelNotFound", "Price level not found.");

        // Unset all others
        var others = await _context.PriceLevels.Where(p => p.TenantId == priceLevel.TenantId && p.Id != request.Id).ToListAsync(cancellationToken);
        foreach (var other in others) other.UnsetDefault();
        priceLevel.SetAsDefault();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivatePriceLevelCommandHandler : IRequestHandler<DeactivatePriceLevelCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivatePriceLevelCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivatePriceLevelCommand request, CancellationToken cancellationToken)
    {
        var priceLevel = await _context.PriceLevels.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (priceLevel == null) return Result.Failure("PriceLevelNotFound", "Price level not found.");
        priceLevel.Deactivate();
        _context.PriceLevels.Update(priceLevel);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdatePriceLevelCommandHandler : IRequestHandler<UpdatePriceLevelCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdatePriceLevelCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdatePriceLevelCommand request, CancellationToken cancellationToken)
    {
        var priceLevel = await _context.PriceLevels.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (priceLevel == null) return Result.Failure("PriceLevelNotFound", "Price level not found.");

        priceLevel.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.SalesChannel);
        _context.PriceLevels.Update(priceLevel);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivatePriceLevelCommandHandler : IRequestHandler<ActivatePriceLevelCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivatePriceLevelCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivatePriceLevelCommand request, CancellationToken cancellationToken)
    {
        var priceLevel = await _context.PriceLevels.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (priceLevel == null) return Result.Failure("PriceLevelNotFound", "Price level not found.");

        priceLevel.Activate();
        _context.PriceLevels.Update(priceLevel);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
