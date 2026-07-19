using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateTaxRegistrationCommand(Guid TenantId, UpsertTaxRegistrationDto Dto) : IRequest<Result<TaxRegistrationProfileDto>>;
public record UpdateTaxRegistrationCommand(Guid Id, UpsertTaxRegistrationDto Dto) : IRequest<Result<TaxRegistrationProfileDto>>;
public record DeleteTaxRegistrationCommand(Guid Id) : IRequest<Result>;
public record UploadTaxRegistrationCertificateCommand(
    Guid Id,
    string FileName,
    Stream Content,
    string? ContentType,
    string? DocumentNumber,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    string? Notes) : IRequest<Result<TaxRegistrationProfileDto>>;
