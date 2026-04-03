namespace RevPay.Domain.Enums;

public enum TaxpayerType { Individual, Corporate }

public enum BillStatus { Pending, PartiallyPaid, Paid, Overdue, Cancelled, Disputed }

public enum PaymentStatus { Pending, Processing, Successful, Failed, Reversed, Disputed }

public enum PaymentChannel { Card, BankTransfer, USSD, POS, BankBranch }

public enum LedgerEntryType { Debit, Credit }
