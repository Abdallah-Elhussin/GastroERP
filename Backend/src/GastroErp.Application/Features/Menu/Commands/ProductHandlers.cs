using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Domain.Entities.Menu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Menu.Commands;

// ─── Product Handlers ─────────────────────────────────────────────────────────

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateProductCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.Dto.CategoryId, cancellationToken);
        if (!categoryExists) return Result<ProductDto>.Failure("CategoryNotFound", "Category not found.");

        var product = new Product(
            request.Dto.TenantId,
            request.Dto.CategoryId,
            request.Dto.NameAr,
            request.Dto.BasePrice,
            request.Dto.Currency,
            request.Dto.NameEn,
            request.Dto.SKU
        );

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product created: {ProductId}", product.Id);
        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(IApplicationDbContext context, ILogger<UpdateProductCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.UpdateInfo(
            request.Dto.NameAr, request.Dto.NameEn,
            request.Dto.DescriptionAr, request.Dto.DescriptionEn,
            request.Dto.SKU, request.Dto.Barcode,
            request.Dto.CaloriesMin, request.Dto.CaloriesMax,
            request.Dto.PrepTimeMinutes
        );
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product updated: {ProductId}", product.Id);
        return Result.Success();
    }
}

public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateProductPriceCommandHandler> _logger;

    public UpdateProductPriceCommandHandler(IApplicationDbContext context, ILogger<UpdateProductPriceCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.UpdatePrice(request.Dto.NewPrice);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product price updated: {ProductId} -> {Price}", product.Id, request.Dto.NewPrice);
        return Result.Success();
    }
}

public class SetProductCategoryCommandHandler : IRequestHandler<SetProductCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetProductCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists) return Result.Failure("CategoryNotFound", "Category not found.");

        product.SetCategory(request.CategoryId);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetProductFeaturedCommandHandler : IRequestHandler<SetProductFeaturedCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetProductFeaturedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetProductFeaturedCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");
        product.SetFeatured(request.IsFeatured);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class MarkProductUnavailableCommandHandler : IRequestHandler<MarkProductUnavailableCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MarkProductUnavailableCommandHandler> _logger;

    public MarkProductUnavailableCommandHandler(IApplicationDbContext context, ILogger<MarkProductUnavailableCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(MarkProductUnavailableCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");
        product.MarkUnavailable(request.Reason);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product marked unavailable: {ProductId}, Reason: {Reason}", product.Id, request.Reason);
        return Result.Success();
    }
}

public class MarkProductAvailableCommandHandler : IRequestHandler<MarkProductAvailableCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkProductAvailableCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(MarkProductAvailableCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");
        product.MarkAvailable();
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetProductPriceLevelCommandHandler : IRequestHandler<SetProductPriceLevelCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SetProductPriceLevelCommandHandler> _logger;

    public SetProductPriceLevelCommandHandler(IApplicationDbContext context, ILogger<SetProductPriceLevelCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(SetProductPriceLevelCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.PriceLevels)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.SetPriceLevel(request.PriceLevelId, request.Price);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product price level set: {ProductId}, Level: {PriceLevelId}, Price: {Price}",
            product.Id, request.PriceLevelId, request.Price);
        return Result.Success();
    }
}

public class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddProductImageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.AddImage(request.ImageUrl, request.ThumbnailUrl, request.AltText, request.IsPrimary);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RemoveProductImageCommandHandler : IRequestHandler<RemoveProductImageCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveProductImageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.RemoveImage(request.ImageId);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetProductPrimaryImageCommandHandler : IRequestHandler<SetProductPrimaryImageCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetProductPrimaryImageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetProductPrimaryImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.SetPrimaryImage(request.ImageId);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class AddModifierGroupCommandHandler : IRequestHandler<AddModifierGroupCommand, Result<ModifierGroupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddModifierGroupCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<ModifierGroupDto>> Handle(AddModifierGroupCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.ModifierGroups).FirstOrDefaultAsync(p => p.Id == request.Dto.ProductId, cancellationToken);
        if (product == null) return Result<ModifierGroupDto>.Failure("ProductNotFound", "Product not found.");

        product.AddModifierGroup(request.Dto.NameAr, request.Dto.NameEn, request.Dto.MinSelection, request.Dto.MaxSelection, request.Dto.IsRequired);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        var newGroup = product.ModifierGroups.LastOrDefault();
        return Result<ModifierGroupDto>.Success(_mapper.Map<ModifierGroupDto>(newGroup));
    }
}

public class RemoveModifierGroupCommandHandler : IRequestHandler<RemoveModifierGroupCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveModifierGroupCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveModifierGroupCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.ModifierGroups).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.RemoveModifierGroup(request.GroupId);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class AddModifierCommandHandler : IRequestHandler<AddModifierCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddModifierCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddModifierCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.ModifierGroups.Include(g => g.Modifiers).FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null) return Result.Failure("ModifierGroupNotFound", "Modifier group not found.");

        group.AddModifier(request.Dto.NameAr, request.Dto.NameEn, request.Dto.ExtraPrice, request.Dto.IsDefault);
        _context.ModifierGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RemoveModifierCommandHandler : IRequestHandler<RemoveModifierCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveModifierCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveModifierCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.ModifierGroups.Include(g => g.Modifiers).FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null) return Result.Failure("ModifierGroupNotFound", "Modifier group not found.");

        group.RemoveModifier(request.ModifierId);
        _context.ModifierGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateModifierGroupCommandHandler : IRequestHandler<DeactivateModifierGroupCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateModifierGroupCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateModifierGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.ModifierGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null) return Result.Failure("ModifierGroupNotFound", "Modifier group not found.");

        group.Deactivate();
        _context.ModifierGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class AddOptionGroupCommandHandler : IRequestHandler<AddOptionGroupCommand, Result<OptionGroupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddOptionGroupCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<OptionGroupDto>> Handle(AddOptionGroupCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.OptionGroups).FirstOrDefaultAsync(p => p.Id == request.Dto.ProductId, cancellationToken);
        if (product == null) return Result<OptionGroupDto>.Failure("ProductNotFound", "Product not found.");

        product.AddOptionGroup(request.Dto.NameAr, request.Dto.NameEn, request.Dto.IsRequired);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        var newGroup = product.OptionGroups.LastOrDefault();
        return Result<OptionGroupDto>.Success(_mapper.Map<OptionGroupDto>(newGroup));
    }
}

public class AddOptionCommandHandler : IRequestHandler<AddOptionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddOptionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddOptionCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.OptionGroups.Include(g => g.Options).FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null) return Result.Failure("OptionGroupNotFound", "Option group not found.");

        group.AddOption(request.Dto.NameAr, request.Dto.NameEn, request.Dto.ExtraPrice, request.Dto.IsDefault);
        _context.OptionGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateOptionGroupCommandHandler : IRequestHandler<DeactivateOptionGroupCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateOptionGroupCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateOptionGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.OptionGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null) return Result.Failure("OptionGroupNotFound", "Option group not found.");

        group.Deactivate();
        _context.OptionGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RemoveOptionGroupCommandHandler : IRequestHandler<RemoveOptionGroupCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveOptionGroupCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveOptionGroupCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.OptionGroups).FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null) return Result.Failure("ProductNotFound", "Product not found.");

        product.RemoveOptionGroup(request.GroupId);
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RemoveOptionCommandHandler : IRequestHandler<RemoveOptionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveOptionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveOptionCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.OptionGroups.Include(g => g.Options).FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null) return Result.Failure("OptionGroupNotFound", "Option group not found.");

        group.RemoveOption(request.OptionId);
        _context.OptionGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateOptionGroupCommandHandler : IRequestHandler<UpdateOptionGroupCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateOptionGroupCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateOptionGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.OptionGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null) return Result.Failure("OptionGroupNotFound", "Option group not found.");

        group.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.IsRequired);
        _context.OptionGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateOptionCommandHandler : IRequestHandler<UpdateOptionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateOptionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await _context.Options.FirstOrDefaultAsync(o => o.Id == request.OptionId, cancellationToken);
        if (option == null) return Result.Failure("OptionNotFound", "Option not found.");

        option.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.ExtraPrice, request.Dto.IsDefault);
        _context.Options.Update(option);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
