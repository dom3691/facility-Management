using Microsoft.AspNetCore.Identity;

namespace FacilityInspection.Persistence.Identity;

/// <summary>
/// Application role for ASP.NET Core Identity, keyed by <see cref="Guid"/>.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName)
        : base(roleName)
    {
    }
}
