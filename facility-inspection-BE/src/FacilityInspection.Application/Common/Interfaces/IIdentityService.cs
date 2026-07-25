using FacilityInspection.Application.Features.Authentication;

namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Abstraction over ASP.NET Core Identity user/role operations. Declared in Application so
/// use cases and controllers stay decoupled from Identity; implemented in Persistence where
/// <c>ApplicationUser</c> and the Identity managers live.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Public self-service registration. Always creates an <c>Initiator</c> — any role supplied
    /// in the request is ignored so an anonymous caller can never grant themselves a privileged
    /// role. Privileged accounts are created only via <see cref="CreateUserAsync"/>.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-only user provisioning. Generates a secure password, flags the account for a
    /// mandatory password change, and emails an invitation link to set a password.
    /// </summary>
    Task<CreateUserResponse> CreateUserAsync(
        AdminCreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<CurrentUserResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Always succeeds from the caller's perspective; sends a reset email when the user exists.</summary>
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the distinct ids of active users in any of the given roles.</summary>
    Task<IReadOnlyList<Guid>> GetUserIdsInRolesAsync(
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>Vendor</c> a user is linked to, or null if none.</summary>
    Task<Guid?> GetUserVendorIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns a user's email address, or null if the user does not exist.</summary>
    Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns the ids of active users linked to the given vendor.</summary>
    Task<IReadOnlyList<Guid>> GetUserIdsByVendorIdAsync(
        Guid vendorId,
        CancellationToken cancellationToken = default);
}
