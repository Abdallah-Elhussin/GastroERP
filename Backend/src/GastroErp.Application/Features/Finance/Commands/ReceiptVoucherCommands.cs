using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateReceiptVoucherCommand(Guid TenantId, UpsertReceiptVoucherDto Dto)
    : IRequest<Result<ReceiptVoucherDto>>;

public record UpdateReceiptVoucherCommand(Guid Id, UpsertReceiptVoucherDto Dto)
    : IRequest<Result<ReceiptVoucherDto>>;

public record SubmitReceiptVoucherCommand(Guid Id) : IRequest<Result<ReceiptVoucherDto>>;

public record ApproveReceiptVoucherCommand(Guid Id, Guid UserId) : IRequest<Result<ReceiptVoucherDto>>;

public record PostReceiptVoucherCommand(Guid Id, Guid UserId)
    : IRequest<Result<ReceiptVoucherDto>>;

public record ReverseReceiptVoucherCommand(Guid Id, Guid UserId)
    : IRequest<Result<ReceiptVoucherDto>>;

public record CancelReceiptVoucherCommand(Guid Id, Guid UserId) : IRequest<Result<ReceiptVoucherDto>>;

public record DeleteReceiptVoucherCommand(Guid Id) : IRequest<Result>;
