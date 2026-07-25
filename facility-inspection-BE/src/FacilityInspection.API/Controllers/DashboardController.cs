using FacilityInspection.Application.DTOs.Dashboard;
using FacilityInspection.Application.Features.Dashboard.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>Operational dashboard counters (role-scoped) for the SPA.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly ISender _mediator;

    public DashboardController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns the summary counters scoped to the current user's role.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return Ok(result);
    }
}
