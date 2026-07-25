using FacilityInspection.Application.DTOs.Dashboard;
using MediatR;

namespace FacilityInspection.Application.Features.Dashboard.GetDashboardSummary;

/// <summary>Returns the role-scoped operational counters for the current user's dashboard.</summary>
public record GetDashboardSummaryQuery : IRequest<DashboardSummaryResponse>;
