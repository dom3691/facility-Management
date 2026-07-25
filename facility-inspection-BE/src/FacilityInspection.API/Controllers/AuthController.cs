using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Features.Authentication;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ValidationException = FacilityInspection.Application.Common.Exceptions.ValidationException;

namespace FacilityInspection.API.Controllers;

/// <summary>
/// Authentication endpoints. Deliberately unversioned so the routes are the stable
/// <c>/api/auth/*</c> the SPA expects.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<AdminCreateUserRequest> _adminCreateUserValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public AuthController(
        IIdentityService identityService,
        ICurrentUserService currentUser,
        IValidator<RegisterRequest> registerValidator,
        IValidator<AdminCreateUserRequest> adminCreateUserValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _identityService = identityService;
        _currentUser = currentUser;
        _registerValidator = registerValidator;
        _adminCreateUserValidator = adminCreateUserValidator;
        _loginValidator = loginValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    /// <summary>Registers a new user and returns an access token.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var response = await _identityService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Admin-only user management: creates a user with an explicit role. A secure password is
    /// generated server-side and an invitation email with a set-password link is sent.
    /// </summary>
    [HttpPost("users")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateUserResponse>> CreateUser(
        AdminCreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _adminCreateUserValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var response = await _identityService.CreateUserAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Authenticates a user and returns an access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var response = await _identityService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Requests a password-reset email. Always returns success to avoid account enumeration.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _forgotPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        await _identityService.RequestPasswordResetAsync(request.Email, cancellationToken);
        return Ok(new { message = "If an account exists for that email, a reset link has been sent." });
    }

    /// <summary>Sets a new password using a reset/invitation token from email.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _resetPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        await _identityService.ResetPasswordAsync(request, cancellationToken);
        return Ok(new { message = "Your password has been updated. You can now sign in." });
    }

    /// <summary>Changes the password for the authenticated user (required on first login when flagged).</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            return Unauthorized();
        }

        var response = await _identityService.ChangePasswordAsync(userId, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns the profile and roles of the authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            return Unauthorized();
        }

        var response = await _identityService.GetCurrentUserAsync(userId, cancellationToken);
        return response is null ? Unauthorized() : Ok(response);
    }
}
