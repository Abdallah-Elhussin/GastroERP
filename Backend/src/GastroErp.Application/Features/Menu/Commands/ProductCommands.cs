using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Menu.Commands;

// ─── Product Commands ─────────────────────────────────────────────────────────
public record CreateProductCommand(CreateProductDto Dto) : IRequest<Result<ProductDto>>;
public record UpdateProductCommand(Guid Id, UpdateProductDto Dto) : IRequest<Result>;
public record UpdateProductPriceCommand(Guid Id, UpdateProductPriceDto Dto) : IRequest<Result>;
public record SetProductCategoryCommand(Guid Id, Guid CategoryId) : IRequest<Result>;
public record SetProductFeaturedCommand(Guid Id, bool IsFeatured) : IRequest<Result>;
public record MarkProductUnavailableCommand(Guid Id, string Reason) : IRequest<Result>;
public record MarkProductAvailableCommand(Guid Id) : IRequest<Result>;
public record AddProductImageCommand(Guid ProductId, string ImageUrl, string? ThumbnailUrl, string? AltText, bool IsPrimary) : IRequest<Result>;
public record SetProductPriceLevelCommand(Guid ProductId, Guid PriceLevelId, decimal Price) : IRequest<Result>;

// ─── ModifierGroup Commands ───────────────────────────────────────────────────
public record AddModifierGroupCommand(CreateModifierGroupDto Dto) : IRequest<Result<ModifierGroupDto>>;
public record RemoveModifierGroupCommand(Guid ProductId, Guid GroupId) : IRequest<Result>;
public record AddModifierCommand(Guid GroupId, AddModifierDto Dto) : IRequest<Result>;
public record RemoveModifierCommand(Guid GroupId, Guid ModifierId) : IRequest<Result>;
public record DeactivateModifierGroupCommand(Guid GroupId) : IRequest<Result>;

// ─── OptionGroup Commands ─────────────────────────────────────────────────────
public record AddOptionGroupCommand(CreateOptionGroupDto Dto) : IRequest<Result<OptionGroupDto>>;
public record AddOptionCommand(Guid GroupId, AddOptionDto Dto) : IRequest<Result>;
public record DeactivateOptionGroupCommand(Guid GroupId) : IRequest<Result>;
public record RemoveOptionGroupCommand(Guid ProductId, Guid GroupId) : IRequest<Result>;
public record RemoveOptionCommand(Guid GroupId, Guid OptionId) : IRequest<Result>;
public record UpdateOptionGroupCommand(Guid GroupId, UpdateOptionGroupDto Dto) : IRequest<Result>;
public record UpdateOptionCommand(Guid OptionId, UpdateOptionDto Dto) : IRequest<Result>;

// ─── Product Image Commands ────────────────────────────────────────────────────
public record RemoveProductImageCommand(Guid ProductId, Guid ImageId) : IRequest<Result>;
public record SetProductPrimaryImageCommand(Guid ProductId, Guid ImageId) : IRequest<Result>;
