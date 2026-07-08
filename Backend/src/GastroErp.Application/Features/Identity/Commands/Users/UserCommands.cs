using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Identity.Commands.Users;

public record CreateUserCommand(CreateUserDto Dto) : IRequest<Result<Guid>>;
public record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : IRequest<Result>;
public record DeleteUserCommand(Guid Id) : IRequest<Result>;
public record RestoreUserCommand(Guid Id) : IRequest<Result>;
public record ActivateUserCommand(Guid Id) : IRequest<Result>;
public record DeactivateUserCommand(Guid Id) : IRequest<Result>;
public record LockUserCommand(Guid Id, DateTimeOffset Until) : IRequest<Result>;
public record UnlockUserCommand(Guid Id) : IRequest<Result>;
public record AdminResetUserPasswordCommand(Guid Id, string NewPassword) : IRequest<Result>;
