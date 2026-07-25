using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Security;
using FacilityInspection.Application.Features.Authentication;
using FacilityInspection.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FacilityInspection.Persistence.Identity;

/// <summary>
/// ASP.NET Core Identity-backed implementation of <see cref="IIdentityService"/>.
/// Lives in Persistence where <see cref="ApplicationUser"/> and the Identity managers exist.
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IAccountEmailService _accountEmailService;
    private readonly IFrontendUrlProvider _frontendUrlProvider;
    private readonly ApplicationDbContext _context;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator,
        IAccountEmailService accountEmailService,
        IFrontendUrlProvider frontendUrlProvider,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _accountEmailService = accountEmailService;
        _frontendUrlProvider = frontendUrlProvider;
        _context = context;
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => CreateUserWithRoleAsync(
            request,
            AppRoles.Initiator,
            mustChangePassword: false,
            vendorId: null,
            cancellationToken);

    public async Task<CreateUserResponse> CreateUserAsync(
        AdminCreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!AppRoles.All.Contains(request.Role))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(AdminCreateUserRequest.Role)] = new[] { $"'{request.Role}' is not a valid role." },
            });
        }

        if (request.Role == AppRoles.Vendor)
        {
            var vendorExists = await _context.Vendors
                .AnyAsync(v => v.Id == request.VendorId && !v.IsDeleted, cancellationToken);
            if (!vendorExists)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    [nameof(AdminCreateUserRequest.VendorId)] = new[] { "Vendor was not found." },
                });
            }
        }
        else if (request.VendorId is not null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(AdminCreateUserRequest.VendorId)] = new[] { "VendorId is only valid for Vendor users." },
            });
        }

        var generatedPassword = PasswordGenerator.Generate();
        var registerPayload = new RegisterRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            SAPId = request.SAPId,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = generatedPassword,
            Role = request.Role,
        };

        var auth = await CreateUserWithRoleAsync(
            registerPayload,
            request.Role,
            mustChangePassword: true,
            vendorId: request.VendorId,
            cancellationToken);

        var invitationSent = await TrySendInvitationEmailAsync(auth.Email, auth.FullName, cancellationToken);

        return new CreateUserResponse
        {
            UserId = auth.UserId,
            Email = auth.Email,
            FullName = auth.FullName,
            Roles = auth.Roles,
            InvitationEmailSent = invitationSent,
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("This account is inactive.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return BuildAuthResponse(user, roles);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new CurrentUserResponse
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            SAPId = user.SAPId,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            RequiresPasswordChange = user.MustChangePassword,
            Roles = roles.ToList(),
        };
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return;
        }

        await SendPasswordResetEmailAsync(user, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new BadRequestException("Invalid reset request.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            throw ToValidationException(result, nameof(ResetPasswordRequest.NewPassword));
        }

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);
    }

    public async Task<AuthResponse> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw ToValidationException(result, nameof(ChangePasswordRequest.NewPassword));
        }

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        return BuildAuthResponse(user, roles);
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsInRolesAsync(
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var ids = new HashSet<Guid>();

        foreach (var role in roles.Distinct())
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            foreach (var user in users.Where(u => u.IsActive))
            {
                ids.Add(user.Id);
            }
        }

        return ids.ToList();
    }

    public async Task<Guid?> GetUserVendorIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.VendorId;
    }

    public async Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email;
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsByVendorIdAsync(
        Guid vendorId,
        CancellationToken cancellationToken = default)
        => await _userManager.Users
            .Where(u => u.VendorId == vendorId && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

    private async Task<AuthResponse> CreateUserWithRoleAsync(
        RegisterRequest request,
        string role,
        bool mustChangePassword,
        Guid? vendorId,
        CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            SAPId = request.SAPId,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            MustChangePassword = mustChangePassword,
            VendorId = vendorId,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw ToValidationException(createResult);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            throw ToValidationException(roleResult);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return BuildAuthResponse(user, roles);
    }

    private async Task<bool> TrySendInvitationEmailAsync(
        string email,
        string fullName,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return false;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = _frontendUrlProvider.GetSetPasswordUrl(email, token);
        await _accountEmailService.SendInvitationEmailAsync(email, fullName, url, cancellationToken);
        return true;
    }

    private async Task SendPasswordResetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var email = user.Email ?? string.Empty;
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = _frontendUrlProvider.GetResetPasswordUrl(email, token);
        await _accountEmailService.SendPasswordResetEmailAsync(email, user.FullName, url, cancellationToken);
    }

    private AuthResponse BuildAuthResponse(ApplicationUser user, IList<string> roles)
    {
        var token = _tokenGenerator.GenerateToken(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.SAPId,
            roles);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            SAPId = user.SAPId,
            Roles = roles.ToList(),
            RequiresPasswordChange = user.MustChangePassword,
        };
    }

    private static ValidationException ToValidationException(
        IdentityResult result,
        string? field = null)
    {
        var errors = result.Errors
            .GroupBy(e => field ?? MapErrorCodeToField(e.Code))
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

        return new ValidationException(errors);
    }

    private static string MapErrorCodeToField(string code) => code switch
    {
        _ when code.Contains("Email", StringComparison.OrdinalIgnoreCase) => nameof(RegisterRequest.Email),
        _ when code.Contains("UserName", StringComparison.OrdinalIgnoreCase) => nameof(RegisterRequest.Email),
        _ when code.Contains("Password", StringComparison.OrdinalIgnoreCase) => nameof(RegisterRequest.Password),
        _ when code.Contains("Role", StringComparison.OrdinalIgnoreCase) => nameof(RegisterRequest.Role),
        _ => "Identity",
    };
}
