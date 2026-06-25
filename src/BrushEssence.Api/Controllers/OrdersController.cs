using BrushEssence.Api.Authorization;
using BrushEssence.Application.Orders;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Checkout and order management. Every endpoint requires authentication; a
/// customer only ever sees their own orders, while status changes are reserved
/// for admins.
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Authorize]
public sealed class OrdersController(
    IOrderService orderService,
    IValidator<CreateOrderRequest> createValidator,
    IValidator<UpdateOrderStatusRequest> statusValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var order = await orderService.CreateFromCartAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetMyOrders(
        CancellationToken cancellationToken)
        => Ok(await orderService.GetMyOrdersAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await orderService.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:guid}/tracking")]
    [ProducesResponseType(typeof(OrderTrackingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderTrackingDto>> GetTracking(Guid id, CancellationToken cancellationToken)
        => Ok(await orderService.GetTrackingAsync(id, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
