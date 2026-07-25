using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using FacilityInspection.Persistence.Context;
using FacilityInspection.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Seed;

/// <summary>
/// Seeds baseline data (roles + reference data). Idempotent — safe to run on every startup.
/// </summary>
public static class ApplicationDbContextSeed
{
    /// <summary>Ensures the canonical application roles (<see cref="AppRoles"/>) exist.</summary>
    public static async Task SeedRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        CancellationToken cancellationToken = default)
    {
        foreach (var roleName in AppRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }
    }

    /// <summary>Seeds sample facilities and locations (idempotent by code / name).</summary>
    public static async Task SeedReferenceDataAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken = default)
    {
        (string Name, string Code)[] facilitySeeds =
        {
            ("Head Office", "HO"),
            ("Plant", "PLANT"),
            ("Warehouse", "WH"),
        };

        foreach (var (name, code) in facilitySeeds)
        {
            if (!await context.Facilities.IgnoreQueryFilters().AnyAsync(f => f.Code == code, cancellationToken))
            {
                context.Facilities.Add(new Facility { Name = name, Code = code, IsActive = true });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        // Seed the sample locations under Head Office.
        var headOffice = await context.Facilities.FirstOrDefaultAsync(f => f.Code == "HO", cancellationToken);
        if (headOffice is null)
        {
            return;
        }

        (string Name, string Code)[] locationSeeds =
        {
            ("Reception", "REC"),
            ("Restroom", "RST"),
            ("Electrical Room", "ELEC"),
            ("HVAC Area", "HVAC"),
            ("Common Area", "COMM"),
        };

        foreach (var (name, code) in locationSeeds)
        {
            if (!await context.Locations.IgnoreQueryFilters()
                    .AnyAsync(l => l.FacilityId == headOffice.Id && l.Name == name, cancellationToken))
            {
                context.Locations.Add(new Location
                {
                    FacilityId = headOffice.Id,
                    Name = name,
                    Code = code,
                    IsActive = true,
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Seeds a sample vendor and one user per role with well-known credentials so the full
    /// workflow can be exercised end-to-end. DEVELOPMENT ONLY — the caller must gate this to the
    /// Development environment. Idempotent (skips any user/vendor that already exists).
    /// </summary>
    public static async Task SeedDevelopmentUsersAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        CancellationToken cancellationToken = default)
    {
        // A sample vendor so the seeded vendor user has work orders to see.
        const string sampleVendorName = "Sample Vendor Co";
        var vendor = await context.Vendors
            .FirstOrDefaultAsync(v => v.Name == sampleVendorName, cancellationToken);

        if (vendor is null)
        {
            vendor = new Vendor
            {
                Name = sampleVendorName,
                Category = VendorCategory.ExternalVendor,
                ContactPerson = "Sam Vendor",
                ContactEmail = "contact@samplevendor.dev",
                ContactPhone = "+10000000000",
                IsActive = true,
            };
            context.Vendors.Add(vendor);
            await context.SaveChangesAsync(cancellationToken);
        }

        await EnsureUserAsync(userManager,
            "admin@fis.local", "Admin@123", "System", "Administrator", "SAP-ADMIN", AppRoles.Admin);
        await EnsureUserAsync(userManager,
            "initiator@fis.local", "Initiator@123", "Ivy", "Initiator", "SAP-INIT", AppRoles.Initiator);
        await EnsureUserAsync(userManager,
            "inspector@fis.local", "Inspector@123", "Ian", "Inspector", "SAP-INSP", AppRoles.Inspector);
        await EnsureUserAsync(userManager,
            "vendor@fis.local", "Vendor@123", "Vera", "Vendor", "SAP-VEND", AppRoles.Vendor, vendor.Id);
    }

    /// <summary>Creates a user (with role and optional vendor link) if the email is not already taken.</summary>
    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string sapId,
        string role,
        Guid? vendorId = null)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            SAPId = sapId,
            IsActive = true,
            VendorId = vendorId,
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
