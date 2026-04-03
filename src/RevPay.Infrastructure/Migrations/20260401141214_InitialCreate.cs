using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RevPay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "revpay");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ledger_entries",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntryType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mdas",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BankAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mdas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentReference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxpayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxpayerEmail = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Channel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GatewayProvider = table.Column<string>(type: "text", nullable: false),
                    GatewayReference = table.Column<string>(type: "text", nullable: true),
                    GatewayAuthCode = table.Column<string>(type: "text", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "receipts",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PdfUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationReports",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GatewayProvider = table.Column<string>(type: "text", nullable: false),
                    InternalTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    GatewayTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Variance = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "taxpayers",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BVN = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    NIN = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TIN = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_taxpayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxpayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    MdaId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginIp = table.Column<string>(type: "text", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "revenue_heads",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MdaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    GlAccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revenue_heads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_revenue_heads_mdas_MdaId",
                        column: x => x.MdaId,
                        principalSchema: "revpay",
                        principalTable: "mdas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bills",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TaxpayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    MdaId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevenueHeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevenueHeadCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentReference = table.Column<string>(type: "text", nullable: true),
                    AssessmentYear = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bills_mdas_MdaId",
                        column: x => x.MdaId,
                        principalSchema: "revpay",
                        principalTable: "mdas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bills_taxpayers_TaxpayerId",
                        column: x => x.TaxpayerId,
                        principalSchema: "revpay",
                        principalTable: "taxpayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "revpay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "revpay",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_bills",
                schema: "revpay",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountApplied = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_bills", x => new { x.PaymentId, x.BillId });
                    table.ForeignKey(
                        name: "FK_payment_bills_bills_BillId",
                        column: x => x.BillId,
                        principalSchema: "revpay",
                        principalTable: "bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_bills_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "revpay",
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAt",
                schema: "revpay",
                table: "audit_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityId",
                schema: "revpay",
                table: "audit_logs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_bills_BillNumber",
                schema: "revpay",
                table: "bills",
                column: "BillNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bills_DueDate",
                schema: "revpay",
                table: "bills",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_bills_MdaId_Status",
                schema: "revpay",
                table: "bills",
                columns: new[] { "MdaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_bills_TaxpayerId",
                schema: "revpay",
                table: "bills",
                column: "TaxpayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_AccountCode",
                schema: "revpay",
                table: "ledger_entries",
                column: "AccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_EntryDate",
                schema: "revpay",
                table: "ledger_entries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_PaymentId",
                schema: "revpay",
                table: "ledger_entries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_mdas_Code",
                schema: "revpay",
                table: "mdas",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_bills_BillId",
                schema: "revpay",
                table: "payment_bills",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_IdempotencyKey",
                schema: "revpay",
                table: "payments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_PaymentReference",
                schema: "revpay",
                table: "payments",
                column: "PaymentReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_Status",
                schema: "revpay",
                table: "payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_payments_TaxpayerId",
                schema: "revpay",
                table: "payments",
                column: "TaxpayerId");

            migrationBuilder.CreateIndex(
                name: "IX_receipts_PaymentId",
                schema: "revpay",
                table: "receipts",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_receipts_ReceiptNumber",
                schema: "revpay",
                table: "receipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ExpiresAt",
                schema: "revpay",
                table: "refresh_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                schema: "revpay",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_revenue_heads_MdaId_Code",
                schema: "revpay",
                table: "revenue_heads",
                columns: new[] { "MdaId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_taxpayers_BVN",
                schema: "revpay",
                table: "taxpayers",
                column: "BVN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_taxpayers_Email",
                schema: "revpay",
                table: "taxpayers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_taxpayers_TIN",
                schema: "revpay",
                table: "taxpayers",
                column: "TIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "revpay",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_MdaId",
                schema: "revpay",
                table: "users",
                column: "MdaId");

            migrationBuilder.CreateIndex(
                name: "IX_users_TaxpayerId",
                schema: "revpay",
                table: "users",
                column: "TaxpayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "ledger_entries",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "payment_bills",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "receipts",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "ReconciliationReports",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "revenue_heads",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "bills",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "users",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "mdas",
                schema: "revpay");

            migrationBuilder.DropTable(
                name: "taxpayers",
                schema: "revpay");
        }
    }
}
