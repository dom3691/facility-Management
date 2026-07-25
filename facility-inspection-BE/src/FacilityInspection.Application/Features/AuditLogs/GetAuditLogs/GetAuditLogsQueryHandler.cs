using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.AuditLogs;
using FacilityInspection.Application.Features.AuditLogs.Common;
using MediatR;

namespace FacilityInspection.Application.Features.AuditLogs.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedResult<AuditLogResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAuditLogsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<AuditLogResponse>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationHelper.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _unitOfWork.AuditLogs.GetPagedAsync(
            pageNumber, pageSize, request.EntityName, request.Action, cancellationToken);

        var mapped = items.Select(a => a.ToResponse()).ToList();
        return PaginatedResult<AuditLogResponse>.Create(mapped, totalCount, pageNumber, pageSize);
    }
}
