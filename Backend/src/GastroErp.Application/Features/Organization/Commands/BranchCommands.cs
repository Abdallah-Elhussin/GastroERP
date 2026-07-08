using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Organization.Commands;

public record CreateBranchCommand(CreateBranchDto Dto) : IRequest<Result<BranchDto>>;
public record UpdateBranchCommand(Guid Id, UpdateBranchDto Dto) : IRequest<Result>;
public record ChangeBranchStatusCommand(Guid Id, bool IsActive) : IRequest<Result>;
public record ArchiveBranchCommand(Guid Id) : IRequest<Result>;
public record RestoreBranchCommand(Guid Id) : IRequest<Result>;
