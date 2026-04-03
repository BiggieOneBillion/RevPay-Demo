using RevPay.Domain.Common;
using RevPay.Domain.Enums;
using RevPay.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace RevPay.Domain.Entities;

public class Taxpayer : AggregateRoot<Guid>
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? BVN { get; private set; }         // Bank Verification Number
    public string? NIN { get; private set; }         // National ID Number
    public TaxpayerType Type { get; private set; }   // Individual | Corporate
    public string? TIN { get; private set; }         // Tax Identification Number
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }
    public Address? Address { get; private set; }     // Value Object
    
    private readonly List<Bill> _bills = new();
    public IReadOnlyCollection<Bill> Bills => _bills.AsReadOnly();

    private Taxpayer() { } // EF Core

    public static Taxpayer Create(string firstName, string lastName,
        string email, string phone, TaxpayerType type)
    {
        var taxpayer = new Taxpayer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            PhoneNumber = phone,
            Type = type,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };
        // taxpayer.AddDomainEvent(new TaxpayerCreatedEvent(taxpayer.Id));
        return taxpayer;
    }

    public void VerifyIdentity(string bvn, string nin)
    {
        BVN = bvn;
        NIN = nin;
        IsVerified = true;
        // AddDomainEvent(new TaxpayerVerifiedEvent(Id));
    }
    
    public void SetAddress(Address address)
    {
        Address = address;
    }
}
