using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.DTOs.AuditLogs;
using FacilityInspection.Application.Features.AuditLogs.Common;
using MediatR;

namespace FacilityInspection.Application.Features.AuditLogs.GetAuditLogsByEntity;

public class GetAuditLogsByEntityQueryHandler
    : IRequestHandler<GetAuditLogsByEntityQuery, IReadOnlyList<AuditLogResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAuditLogsByEntityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AuditLogResponse>> Handle(
        GetAuditLogsByEntityQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _unitOfWork.AuditLogs.GetByEntityAsync(request.EntityName, request.EntityId, cancellationToken);
        return logs.Select(a => a.ToResponse()).ToList();
    }
}
