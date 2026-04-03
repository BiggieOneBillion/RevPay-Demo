using RevPay.Domain.Entities;
using RevPay.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface ITaxpayerRepository
{
    Task<Taxpayer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Taxpayer taxpayer, CancellationToken ct = default);
}

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Bill?> GetByBillNumberAsync(string billNumber, CancellationToken ct = default);
    Task<List<Bill>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<List<Bill>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<(List<Bill> Items, int Total)> GetPagedAsync(
        Guid taxpayerId, BillStatus? status, Guid? mdaId,
        int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Bill bill, CancellationToken ct = default);
}

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Payment?> GetByReferenceAsync(string reference, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task AddRefreshTokenAsync(AppRefreshToken token, CancellationToken ct = default);
    Task<List<AppRefreshToken>> GetActiveRefreshTokensAsync(string ip, CancellationToken ct = default);
    Task<List<AppRefreshToken>> GetAllActiveRefreshTokensAsync(CancellationToken ct = default);
}
