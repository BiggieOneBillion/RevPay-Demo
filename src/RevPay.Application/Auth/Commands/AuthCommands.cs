using MediatR;
using RevPay.Application.Common.Exceptions;
using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;

namespace RevPay.Application.Auth.Commands;

// ─── Register ────────────────────────────────────────────────────────────────

public record RegisterCommand(
    string FirstName, string LastName,
    string Email, string PhoneNumber,
    string Password
) : IRequest<AuthResult>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IUserRepository _users;
    private readonly ITaxpayerRepository _taxpayers;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public RegisterCommandHandler(IUserRepository users, ITaxpayerRepository taxpayers,
        IUnitOfWork uow, IJwtService jwt)
    {
        _users = users; _taxpayers = taxpayers; _uow = uow; _jwt = jwt;
    }

    public async Task<AuthResult> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        if (await _users.ExistsByEmailAsync(cmd.Email, ct))
            throw new BusinessRuleException("An account with this email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password, workFactor: 12);

        // Create linked Taxpayer profile
        var taxpayer = Taxpayer.Create(cmd.FirstName, cmd.LastName, cmd.Email, cmd.PhoneNumber,
            RevPay.Domain.Enums.TaxpayerType.Individual);
        await _taxpayers.AddAsync(taxpayer, ct);

        // Create User for auth
        var user = User.Create(cmd.Email, passwordHash, "Taxpayer", taxpayerId: taxpayer.Id);
        await _users.AddAsync(user, ct);

        await _uow.SaveChangesAsync(ct);

        var claims = new UserClaims
        {
            UserId = user.Id, Email = user.Email,
            TaxpayerId = taxpayer.Id, Role = user.Role
        };
        var accessToken = _jwt.GenerateAccessToken(claims);
        var refresh = _jwt.GenerateRefreshToken(user.Id, "registration");
        await _users.AddRefreshTokenAsync(new AppRefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(refresh.RawToken),
            ExpiresAt = refresh.ExpiresAt,
            CreatedByIp = "registration",
            CreatedAt = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthResult(accessToken, refresh.RawToken, user.Email, user.Role);
    }
}

// ─── Login ────────────────────────────────────────────────────────────────────

public record LoginCommand(string Email, string Password, string IpAddress) : IRequest<AuthResult>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public LoginCommandHandler(IUserRepository users, IUnitOfWork uow, IJwtService jwt)
    {
        _users = users; _uow = uow; _jwt = jwt;
    }

    public async Task<AuthResult> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(cmd.Email, ct)
            ?? throw new BusinessRuleException("Invalid email or password.");

        if (!user.IsActive)
            throw new BusinessRuleException("Account is deactivated. Please contact support.");

        if (!BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            throw new BusinessRuleException("Invalid email or password.");

        user.RecordLogin(cmd.IpAddress);

        var claims = new UserClaims
        {
            UserId = user.Id, Email = user.Email,
            TaxpayerId = user.TaxpayerId, MdaId = user.MdaId, Role = user.Role
        };
        var accessToken = _jwt.GenerateAccessToken(claims);
        var refresh = _jwt.GenerateRefreshToken(user.Id, cmd.IpAddress);
        await _users.AddRefreshTokenAsync(new AppRefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(refresh.RawToken),
            ExpiresAt = refresh.ExpiresAt,
            CreatedByIp = cmd.IpAddress,
            CreatedAt = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthResult(accessToken, refresh.RawToken, user.Email, user.Role);
    }
}

// ─── Refresh ──────────────────────────────────────────────────────────────────

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : IRequest<AuthResult>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(IUserRepository users, IUnitOfWork uow, IJwtService jwt)
    {
        _users = users; _uow = uow; _jwt = jwt;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var tokens = await _users.GetActiveRefreshTokensAsync(cmd.IpAddress, ct);
        AppRefreshToken? matched = null;
        User? user = null;

        foreach (var t in tokens)
        {
            if (BCrypt.Net.BCrypt.Verify(cmd.RefreshToken, t.TokenHash))
            {
                matched = t;
                user = await _users.GetByIdAsync(t.UserId, ct);
                break;
            }
        }

        if (matched is null || user is null || !matched.IsActive)
            throw new BusinessRuleException("Invalid or expired refresh token.");

        // Rotate — revoke old
        matched.RevokedAt = DateTime.UtcNow;

        var claims = new UserClaims
        {
            UserId = user.Id, Email = user.Email,
            TaxpayerId = user.TaxpayerId, MdaId = user.MdaId, Role = user.Role
        };
        var newAccess = _jwt.GenerateAccessToken(claims);
        var newRefresh = _jwt.GenerateRefreshToken(user.Id, cmd.IpAddress);
        await _users.AddRefreshTokenAsync(new AppRefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(newRefresh.RawToken),
            ExpiresAt = newRefresh.ExpiresAt,
            CreatedByIp = cmd.IpAddress,
            CreatedAt = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthResult(newAccess, newRefresh.RawToken, user.Email, user.Role);
    }
}

// ─── Logout ───────────────────────────────────────────────────────────────────

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public LogoutCommandHandler(IUserRepository users, IUnitOfWork uow)
    {
        _users = users; _uow = uow;
    }

    public async Task<Unit> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var tokens = await _users.GetAllActiveRefreshTokensAsync(ct);
        foreach (var t in tokens)
        {
            if (BCrypt.Net.BCrypt.Verify(cmd.RefreshToken, t.TokenHash))
            {
                t.RevokedAt = DateTime.UtcNow;
                break;
            }
        }
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ─── Shared Result ────────────────────────────────────────────────────────────

public record AuthResult(string AccessToken, string RefreshToken, string Email, string Role);
