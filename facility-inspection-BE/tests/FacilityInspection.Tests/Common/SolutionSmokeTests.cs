using FacilityInspection.Domain.Common;
using FluentAssertions;

namespace FacilityInspection.Tests.Common;

/// <summary>
/// Placeholder tests proving the test harness and project references are wired up.
/// Replace with real unit/integration tests as features are implemented.
/// </summary>
public class SolutionSmokeTests
{
    private sealed class SampleEntity : AuditableEntity;

    [Fact]
    public void BaseEntity_Assigns_NonEmpty_Id_On_Construction()
    {
        var entity = new SampleEntity();

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void BaseEntity_DomainEvents_Start_Empty()
    {
        var entity = new SampleEntity();

        entity.DomainEvents.Should().BeEmpty();
    }
}
