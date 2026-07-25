namespace FacilityInspection.Domain.Enums;

/// <summary>
/// Outcome an inspector assigns to an inspected asset. Anything other than
/// <see cref="Good"/> typically routes the incident to vendor assignment.
/// </summary>
public enum InspectionClassification
{
    Good = 1,
    Faulty = 2,
    RunDown = 3,
    Damaged = 4,
}
