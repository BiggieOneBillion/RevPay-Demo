using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevPay.API.Models;
using RevPay.Application.Bills.Queries;
using RevPay.Domain.Enums;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RevPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BillsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BillsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all bills belonging to the authenticated taxpayer, paginated.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyBills([FromQuery] BillQueryParams query)
    {
        var result = await _mediator.Send(new GetTaxpayerBillsQuery(
            GetTaxpayerId(), query.Status, query.MdaId, query.Page, query.PageSize));
        return Ok(ApiResponse<PagedResult<BillSummaryDto>>.SuccessResponse(result));
    }

    /// <summary>Lookup a bill by its bill number.</summary>
    [HttpGet("{billNumber}")]
    public async Task<IActionResult> GetByBillNumber(string billNumber)
    {
        var result = await _mediator.Send(new GetBillByNumberQuery(billNumber, GetTaxpayerId()));
        return Ok(ApiResponse<BillDetailDto>.SuccessResponse(result));
    }

    /// <summary>Create a new bill (MDA officers only).</summary>
    [HttpPost]
    [Authorize(Roles = "MdaOfficer,MdaAdmin,SystemAdmin")]
    public async Task<IActionResult> CreateBill([FromBody] CreateBillRequest req)
    {
        var result = await _mediator.Send(new CreateBillCommand(
            req.TaxpayerId, req.MdaId, req.RevenueHeadId, req.RevenueHeadCode,
            req.Description, req.Amount, req.DueDate, req.AssessmentYear));
        return Ok(ApiResponse<BillDetailDto>.SuccessResponse(result, "Bill created successfully."));
    }

    private Guid GetTaxpayerId()
    {
        var id = User.FindFirstValue("taxpayer_id");
        return string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id);
    }
}

public class BillQueryParams
{
    public BillStatus? Status { get; set; }
    public Guid? MdaId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record CreateBillRequest(
    Guid TaxpayerId, Guid MdaId, Guid RevenueHeadId,
    string RevenueHeadCode, string Description, decimal Amount,
    DateTime DueDate, int AssessmentYear);
