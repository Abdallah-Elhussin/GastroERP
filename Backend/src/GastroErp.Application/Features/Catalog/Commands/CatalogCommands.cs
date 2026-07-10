using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Catalog.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Catalog.Commands;

public record CreateCatalogDraftCommand(Guid TenantId, CreateCatalogDraftDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record UpdateCatalogGeneralInfoCommand(Guid Id, UpdateCatalogGeneralInfoDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record SaveCatalogInventoryCommand(Guid Id, SaveCatalogInventoryDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record SaveCatalogRecipeCommand(Guid Id, SaveCatalogRecipeDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record SaveCatalogPosCommand(Guid Id, SaveCatalogPosDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record SaveCatalogPricingCommand(Guid Id, SaveCatalogPricingDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record SaveCatalogExtensionsCommand(Guid Id, SaveCatalogExtensionsDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record SaveCatalogRelationshipsCommand(Guid Id, SaveCatalogRelationshipsDto Dto) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record ActivateCatalogDefinitionCommand(Guid Id) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record ImportCatalogDefinitionsCommand(Guid TenantId, IReadOnlyList<CatalogImportRowDto> Rows) : IRequest<Result<int>>;
