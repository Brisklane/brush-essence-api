using BrushEssence.Application.Carts;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// The current caller's shopping cart. Open to guests and signed-in customers
/// alike: signed-in callers are identified by their bearer token, guests by the
/// opaque token on the <c>X-Cart-Token</c> header. There is no per-id route — a
/// caller can only ever see and mutate their own cart.
/// </summary>
[ApiController]
[Route("api/cart")]
[Produces("application/json")]
public sealed class CartController(
    ICartService cartService,
    IValidator<AddCartItemRequest> addValidator,
    IValidator<UpdateCartItemRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> Get(CancellationToken cancellationToken)
        => Ok(await cartService.GetCartAsync(cancellationToken));

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> AddItem(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        await addValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await cartService.AddItemAsync(request, cancellationToken));
    }

    [HttpPut("items/{paintingId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> UpdateItem(
        Guid paintingId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await cartService.UpdateItemAsync(paintingId, request, cancellationToken));
    }

    [HttpDelete("items/{paintingId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> RemoveItem(
        Guid paintingId,
        CancellationToken cancellationToken)
        => Ok(await cartService.RemoveItemAsync(paintingId, cancellationToken));

    [HttpDelete]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> Clear(CancellationToken cancellationToken)
        => Ok(await cartService.ClearAsync(cancellationToken));
}
