namespace FacilityInspection.Domain.Common;

/// <summary>
/// Base type for domain events. Kept infrastructure-free: the Application layer
/// adapts these to a mediator notification when dispatching.
/// </summary>
public abstract class BaseDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; protected set; } = DateTimeOffset.UtcNow;
}
