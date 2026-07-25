using FacilityInspection.Application.Common.Interfaces;

namespace FacilityInspection.Infrastructure.Services;

/// <summary>
/// System-clock implementation of <see cref="IDateTimeProvider"/>.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
