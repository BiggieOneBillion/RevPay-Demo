using RevPay.Domain.Common;
using System;
using System.Collections.Generic;

namespace RevPay.Domain.Entities;

public class MDA : AggregateRoot<Guid>
{
    public string Code { get; private set; } // e.g. LIRS, MOT
    public string Name { get; private set; }
    public string BankAccount { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<RevenueHead> _revenueHeads = new();
    public IReadOnlyCollection<RevenueHead> RevenueHeads => _revenueHeads.AsReadOnly();

    private MDA() { }

    public static MDA Create(string code, string name, string bankAccount)
    {
        return new MDA
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            BankAccount = bankAccount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class RevenueHead : BaseEntity
{
    public Guid MdaId { get; private set; }
    public string Code { get; private set; } // e.g. LUC-001
    public string Name { get; private set; }
    public string GlAccountCode { get; private set; } // General Ledger Account Code

    private RevenueHead() { }

    public static RevenueHead Create(Guid mdaId, string code, string name, string glAccountCode)
    {
        return new RevenueHead
        {
            Id = Guid.NewGuid(),
            MdaId = mdaId,
            Code = code,
            Name = name,
            GlAccountCode = glAccountCode,
            CreatedAt = DateTime.UtcNow
        };
    }
}
