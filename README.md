# RevPay Nigeria — Government Revenue Payment Platform

RevPay is a production-grade revenue collection and payment gateway designed for Nigerian government agencies (MDAs). Built on **.NET 9** using **Clean Architecture** and **CQRS**.

## 🚀 Core Features
- **Idempotent Payments**: Ensures safe retries across unstable networks.
- **Multi-Gateway Support**: Integrated with Paystack and Flutterwave.
- **Secure Webhooks**: HMAC-SHA512 verification for automated payment confirmation.
- **Resilience**: Built-in circuit breakers and exponential backoffs via Polly.

## 🛠 Tech Stack
- **Backend**: .NET 9, EF Core, MediatR
- **Database**: PostgreSQL
- **Caching/State**: Redis
- **Background Jobs**: Hangfire

