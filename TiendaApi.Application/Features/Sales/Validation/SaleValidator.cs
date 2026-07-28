using FluentValidation;
using TiendaApi.Application.Features.Sales.Request;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Application.Features.Sales.Validation;

public class CreateSaleValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.PaymentType)
            .IsInEnum().WithMessage("Tipo de pago inválido.");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .When(x => x.PaymentType == SalePaymentType.Fiado)
            .WithMessage("El cliente es obligatorio para ventas al fiado.");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("La venta no puede estar vacia.")
            .Must(items => items.Count > 0)
            .WithMessage("La venta debe tener al menos un producto.");

        RuleForEach(x => x.Details).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("El producto es obligatorio.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
        });
    }
}