using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, Result<DeviceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateDeviceCommandHandler> _logger;

    public CreateDeviceCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateDeviceCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<DeviceDto>> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = new Device(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.DeviceType,
            request.Dto.SerialNumber,
            request.Dto.MacAddress,
            request.Dto.NameEn
        );

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Device registered: {DeviceId}", device.Id);

        return Result<DeviceDto>.Success(_mapper.Map<DeviceDto>(device));
    }
}

public class UpdateDeviceCommandHandler : IRequestHandler<UpdateDeviceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateDeviceCommandHandler> _logger;

    public UpdateDeviceCommandHandler(IApplicationDbContext context, ILogger<UpdateDeviceCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device == null) return Result.Failure("DeviceNotFound", "Device not found.");

        device.Rename(request.Dto.NameAr, request.Dto.NameEn);
        _context.Devices.Update(device);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Device updated: {DeviceId}", device.Id);

        return Result.Success();
    }
}

public class ActivateDeviceCommandHandler : IRequestHandler<ActivateDeviceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ActivateDeviceCommandHandler> _logger;

    public ActivateDeviceCommandHandler(IApplicationDbContext context, ILogger<ActivateDeviceCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device == null) return Result.Failure("DeviceNotFound", "Device not found.");

        device.Activate(request.BranchId);
        _context.Devices.Update(device);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Device activated: {DeviceId}", device.Id);

        return Result.Success();
    }
}

public class DeactivateDeviceCommandHandler : IRequestHandler<DeactivateDeviceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateDeviceCommandHandler> _logger;

    public DeactivateDeviceCommandHandler(IApplicationDbContext context, ILogger<DeactivateDeviceCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device == null) return Result.Failure("DeviceNotFound", "Device not found.");

        device.Deactivate();
        _context.Devices.Update(device);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Device deactivated: {DeviceId}", device.Id);

        return Result.Success();
    }
}

public class AssignDeviceCommandHandler : IRequestHandler<AssignDeviceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AssignDeviceCommandHandler> _logger;

    public AssignDeviceCommandHandler(IApplicationDbContext context, ILogger<AssignDeviceCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(AssignDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);
        if (device == null) return Result.Failure("DeviceNotFound", "Device not found.");

        var branchDevice = await _context.BranchDevices
            .FirstOrDefaultAsync(bd => bd.DeviceId == request.DeviceId && bd.BranchId == request.BranchId, cancellationToken);

        if (branchDevice == null)
        {
            branchDevice = new BranchDevice(request.BranchId, request.DeviceId, device.TenantId, request.AssignedBy);
            _context.BranchDevices.Add(branchDevice);
        }
        else
        {
            branchDevice.Activate();
            _context.BranchDevices.Update(branchDevice);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Device {DeviceId} assigned to Branch {BranchId}", request.DeviceId, request.BranchId);

        return Result.Success();
    }
}

public class UnassignDeviceCommandHandler : IRequestHandler<UnassignDeviceCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UnassignDeviceCommandHandler> _logger;

    public UnassignDeviceCommandHandler(IApplicationDbContext context, ILogger<UnassignDeviceCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UnassignDeviceCommand request, CancellationToken cancellationToken)
    {
        var branchDevice = await _context.BranchDevices
            .FirstOrDefaultAsync(bd => bd.DeviceId == request.DeviceId && bd.BranchId == request.BranchId, cancellationToken);

        if (branchDevice == null) return Result.Failure("BranchDeviceNotFound", "Device is not assigned to this branch.");

        branchDevice.Unassign(request.UnassignedBy);
        _context.BranchDevices.Update(branchDevice);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Device {DeviceId} unassigned from Branch {BranchId}", request.DeviceId, request.BranchId);

        return Result.Success();
    }
}
