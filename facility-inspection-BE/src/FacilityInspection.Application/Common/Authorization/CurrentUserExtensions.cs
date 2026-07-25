using FacilityInspection.Application.Common.Interfaces;

namespace FacilityInspection.Application.Common.Authorization;

/// <summary>
/// Convenience helpers over <see cref="ICurrentUserService"/> that centralise the small but
/// error-prone identity chores (parsing the user id, testing roles) so every handler and
/// authorization check performs them the same way instead of re-implementing
/// <c>Guid.TryParse(...)</c> / <c>Roles.Contains(...)</c> inline.
/// </summary>
public static class CurrentUserExtensions
{
    /// <summary>The caller's id as a <see cref="Guid"/>, or <c>null</c> if unauthenticated/unparsable.</summary>
    public static Guid? GetUserId(this ICurrentUserService currentUser)
        => Guid.TryParse(currentUser.UserId, out var id) ? id : null;

    /// <summary>True if the caller holds the given role.</summary>
    public static bool IsInRole(this ICurrentUserService currentUser, string role)
        => currentUser.Roles.Contains(role);

    /// <summary>True if the caller holds any of the given roles.</summary>
    public static bool IsInAnyRole(this ICurrentUserService currentUser, params string[] roles)
        => currentUser.Roles.Any(roles.Contains);

    /// <summary>True if the caller is an Admin.</summary>
    public static bool IsAdmin(this ICurrentUserService currentUser)
        => currentUser.IsInRole(AppRoles.Admin);

    /// <summary>
    /// True if the caller is an Admin or Inspector — the two roles with cross-cutting,
    /// non-owner read access to the operational data (incidents, inspections, work orders).
    /// </summary>
    public static bool IsAdminOrInspector(this ICurrentUserService currentUser)
        => currentUser.IsInAnyRole(AppRoles.Admin, AppRoles.Inspector);
}
