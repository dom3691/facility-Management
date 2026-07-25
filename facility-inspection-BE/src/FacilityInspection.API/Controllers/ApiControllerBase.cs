using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>
/// Base controller for all versioned API endpoints. Establishes the versioned route
/// convention and lazily resolves the MediatR sender so derived controllers stay thin.
/// Feature controllers inherit from this and dispatch commands/queries via <see cref="Mediator"/>.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
