using Microsoft.EntityFrameworkCore;
using RevPay.Domain.Enums;
using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
}

public class TaxpayerRepository : ITaxpayerRepository
{
    private readonly AppDbContext _context;

    public TaxpayerRepository(AppDbContext context) => _context = context;

    public async Task<Taxpayer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Taxpayers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Taxpayer taxpayer, CancellationToken ct = default)
        => await _context.Taxpayers.AddAsync(taxpayer, ct);
}

public class BillRepository : IBillRepository
{
    private readonly AppDbContext _context;

    public BillRepository(AppDbContext context) => _context = context;

    public async Task<Bill?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Bills.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Bill>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => await _context.Bills.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    public async Task<List<Bill>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        var billIds = await _context.Payments
            .Where(p => p.Id == paymentId)
            .SelectMany(p => p.PaymentBills)
            .Select(pb => pb.BillId)
            .ToListAsync(ct);

        return await _context.Bills.Where(b => billIds.Contains(b.Id)).ToListAsync(ct);
    }

    public async Task<Bill?> GetByBillNumberAsync(string billNumber, CancellationToken ct = default)
        => await _context.Bills.FirstOrDefaultAsync(x => x.BillNumber == billNumber, ct);

    public async Task<(List<Bill> Items, int Total)> GetPagedAsync(
        Guid taxpayerId, BillStatus? status, Guid? mdaId,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Bills
            .Where(b => b.TaxpayerId == taxpayerId);

        if (status.HasValue) query = query.Where(b => b.Status == status.Value);
        if (mdaId.HasValue)  query = query.Where(b => b.MdaId == mdaId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.IssueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Bill bill, CancellationToken ct = default)
        => await _context.Bills.AddAsync(bill, ct);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context) => _context = context;

    public async Task<RevPay.Domain.Entities.Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Payments.Include(p => p.PaymentBills).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<RevPay.Domain.Entities.Payment?> GetByReferenceAsync(string reference, CancellationToken ct = default)
        => await _context.Payments.Include(p => p.PaymentBills).FirstOrDefaultAsync(x => x.GatewayReference == reference || x.PaymentReference == reference, ct);

    public async Task AddAsync(RevPay.Domain.Entities.Payment payment, CancellationToken ct = default)
        => await _context.Payments.AddAsync(payment, ct);
}
