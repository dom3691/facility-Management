namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the system clock so handlers stay deterministic and testable.
/// Implemented in Infrastructure.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
