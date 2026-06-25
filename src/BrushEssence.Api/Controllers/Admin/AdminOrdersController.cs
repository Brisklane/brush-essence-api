using BrushEssence.Api.Authorization;
using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Models;
using BrushEssence.Application.Orders;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers.Admin;

/// <summary>Admin order management: list across all customers and advance status.</summary>
[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class AdminOrdersController(
    IOrderService orderService,
    IValidator<UpdateOrderStatusRequest> statusValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminOrderListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminOrderListItemDto>>> GetAll(
        [FromQuery] AdminOrderQuery query,
        CancellationToken cancellationToken)
        => Ok(await orderService.GetAllAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await orderService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        await statusValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await orderService.UpdateStatusAsync(id, request, cancellationToken));
    }
}
