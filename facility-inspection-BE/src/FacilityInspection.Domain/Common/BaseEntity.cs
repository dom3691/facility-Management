using System.ComponentModel.DataAnnotations.Schema;

namespace FacilityInspection.Domain.Common;

/// <summary>
/// Base type for all domain entities. Provides a strongly-typed identity and a
/// lightweight domain-event collection so aggregates can raise events without
/// depending on any infrastructure.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    private readonly List<BaseDomainEvent> _domainEvents = new();

    /// <summary>Transient domain events; never persisted (see <see cref="NotMappedAttribute"/>).</summary>
    [NotMapped]
    public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(BaseDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
