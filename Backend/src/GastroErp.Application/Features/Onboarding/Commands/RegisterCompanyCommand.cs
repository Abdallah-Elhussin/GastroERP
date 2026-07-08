using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Onboarding.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Onboarding.Commands;

public record RegisterCompanyCommand(RegisterCompanyDto Dto) : IRequest<Result<RegisterCompanyResponseDto>>;
