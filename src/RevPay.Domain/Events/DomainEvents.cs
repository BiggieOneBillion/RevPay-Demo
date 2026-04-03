using RevPay.Domain.Common;
using System;
using System.Collections.Generic;
using RevPay.Domain.Entities;

namespace RevPay.Domain.Events;

public record TaxpayerCreatedEvent(Guid TaxpayerId) : IDomainEvent;
public record TaxpayerVerifiedEvent(Guid TaxpayerId) : IDomainEvent;
public record BillPaidEvent(Guid BillId, Guid TaxpayerId, decimal Amount) : IDomainEvent;
public record PenaltyAppliedEvent(Guid BillId, decimal Amount) : IDomainEvent;
public record PaymentCompletedEvent(Guid PaymentId, Guid TaxpayerId, decimal Amount, string TaxpayerEmail, IEnumerable<PaymentBill> PaymentBills) : IDomainEvent;
public record PaymentFailedEvent(Guid PaymentId, string Reason) : IDomainEvent;
public record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent;
