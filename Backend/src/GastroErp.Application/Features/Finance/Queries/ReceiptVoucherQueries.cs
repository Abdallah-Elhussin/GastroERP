using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetReceiptVouchersQuery(Guid TenantId, ReceiptVoucherFilterDto Filter)
    : IRequest<PagedResult<ReceiptVoucherDto>>;

public record GetReceiptVoucherByIdQuery(Guid Id) : IRequest<Result<ReceiptVoucherDto>>;
