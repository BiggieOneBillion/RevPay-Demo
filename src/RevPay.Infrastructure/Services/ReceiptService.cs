using RevPay.Application.Common.Interfaces;
using RevPay.Domain.Entities;
using RevPay.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Services;

public class ReceiptService : IReceiptService
{
    private readonly AppDbContext _db;

    public ReceiptService(AppDbContext db) => _db = db;

    public async Task<Receipt> GenerateAsync(Guid paymentId, CancellationToken ct)
    {
        var receiptNumber = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        var pdfUrl = $"https://cdn.revpay.gov.ng/receipts/{receiptNumber}.pdf";

        var receipt = Receipt.Create(paymentId, receiptNumber, pdfUrl);
        await _db.Receipts.AddAsync(receipt, ct);
        return receipt;
    }
}
