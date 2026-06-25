using BrushEssence.Application.Orders;
using FluentValidation.TestHelper;
using Xunit;

namespace BrushEssence.UnitTests.Orders;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    private static ShippingAddressInput ValidAddress() => new()
    {
        FullName = "Ada Lovelace",
        Line1 = "12 Analytical Way",
        City = "London",
        PostalCode = "EC1A 1AA",
        Country = "United Kingdom",
    };

    [Fact]
    public void Valid_request_passes_validation()
    {
        var request = new CreateOrderRequest { ShippingAddress = ValidAddress() };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Missing_address_lines_fail_validation()
    {
        var request = new CreateOrderRequest
        {
            ShippingAddress = new ShippingAddressInput { FullName = "", Line1 = "", City = "", PostalCode = "", Country = "" },
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("ShippingAddress.FullName");
        result.ShouldHaveValidationErrorFor("ShippingAddress.Line1");
        result.ShouldHaveValidationErrorFor("ShippingAddress.City");
        result.ShouldHaveValidationErrorFor("ShippingAddress.PostalCode");
        result.ShouldHaveValidationErrorFor("ShippingAddress.Country");
    }
}

public class UpdateOrderStatusRequestValidatorTests
{
    private readonly UpdateOrderStatusRequestValidator _validator = new();

    [Fact]
    public void Out_of_range_status_fails_validation()
    {
        var request = new UpdateOrderStatusRequest { Status = (Domain.Entities.OrderStatus)99 };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Status);
    }
}
