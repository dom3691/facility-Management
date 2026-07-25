namespace FacilityInspection.Domain.Enums;

/// <summary>
/// Verifier's decision after reviewing completed work. <see cref="NotFixed"/>
/// typically reopens the work order for further remediation.
/// </summary>
public enum VerificationDecision
{
    Fixed = 1,
    NotFixed = 2,
}
