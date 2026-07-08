using GastroErp.Application.Common.Responses;
using MediatR;

namespace GastroErp.Application.Features.Organization.Commands;

public record DeleteTenantCommand(Guid Id, string Reason) : IRequest<Result>;
