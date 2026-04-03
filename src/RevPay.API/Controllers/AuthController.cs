using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevPay.API.Models;
using RevPay.Application.Auth.Commands;
using System.Threading.Tasks;

namespace RevPay.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Register a new taxpayer account.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _mediator.Send(new RegisterCommand(
            req.FirstName, req.LastName, req.Email, req.PhoneNumber, req.Password));
        return Ok(ApiResponse<AuthResult>.SuccessResponse(result, "Account created successfully."));
    }

    /// <summary>Login and receive JWT + refresh token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _mediator.Send(new LoginCommand(req.Email, req.Password, ip));
        return Ok(ApiResponse<AuthResult>.SuccessResponse(result));
    }

    /// <summary>Refresh access token using a valid refresh token.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _mediator.Send(new RefreshTokenCommand(req.RefreshToken, ip));
        return Ok(ApiResponse<AuthResult>.SuccessResponse(result));
    }

    /// <summary>Revoke the refresh token (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        await _mediator.Send(new LogoutCommand(req.RefreshToken));
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }
}

// Request models
public record RegisterRequest(string FirstName, string LastName,
    string Email, string PhoneNumber, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
