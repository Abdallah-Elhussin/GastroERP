using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Onboarding.DTOs;
using GastroErp.Application.Common.Responses;
using MediatR;

namespace GastroErp.Application.Features.Onboarding.Commands;

public record RegisterCompanyCommand(RegisterCompanyDto Dto)
    : IRequest<Result<RegisterCompanyResponseDto>>, ITransactionalRequest;
