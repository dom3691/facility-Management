using System.ComponentModel.DataAnnotations.Schema;
using FacilityInspection.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace FacilityInspection.Persistence.Identity;

/// <summary>
/// Application user for ASP.NET Core Identity, keyed by <see cref="Guid"/> to match
/// the domain entity identity strategy. <c>Email</c> and <c>PhoneNumber</c> are inherited
/// from <see cref="IdentityUser{TKey}"/>.
/// <para>
/// Identity types live in the Persistence layer (not Domain) so the Domain stays free
/// of the ASP.NET Core Identity dependency. Domain entities reference users by
/// <see cref="Guid"/> (logical foreign keys), not navigation properties.
/// </para>
/// Implements <see cref="IAuditableEntity"/> so the auditing interceptor stamps it too.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>SAP personnel identifier — unique per user.</summary>
    public string SAPId { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When true the user must change their password before using the application
    /// (admin-provisioned accounts and post-reset flows).
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>For a vendor-role user, the <c>Vendor</c> they belong to (logical reference).</summary>
    public Guid? VendorId { get; set; }

    /// <summary>Convenience display name; not persisted.</summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    // IAuditableEntity
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
}
