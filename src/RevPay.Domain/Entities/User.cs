using RevPay.Domain.Common;
using RevPay.Domain.Events;
using System;

namespace RevPay.Domain.Entities;

public class User : AggregateRoot<Guid>
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }           // Taxpayer, MdaOfficer, MdaAdmin, SystemAdmin, AuditViewer
    public Guid? TaxpayerId { get; private set; }
    public Guid? MdaId { get; private set; }
    public bool IsActive { get; private set; }
    public string? LastLoginIp { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string role,
        Guid? taxpayerId = null, Guid? mdaId = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            TaxpayerId = taxpayerId,
            MdaId = mdaId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email));
        return user;
    }

    public void RecordLogin(string ipAddress)
    {
        LastLoginIp = ipAddress;
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

public class AppRefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string CreatedByIp { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
