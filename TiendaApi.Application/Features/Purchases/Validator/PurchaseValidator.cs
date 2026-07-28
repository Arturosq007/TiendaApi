using FluentValidation;
using TiendaApi.Application.Features.Purchases.Request;

namespace TiendaApi.Application.Features.Purchases.Validator;
public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseValidator()
    {
        RuleFor(x => x.PaymentType)
            .IsInEnum().WithMessage("Tipo de pago inválido.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La compra debe tener al menos un producto.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("El producto es obligatorio.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

            item.RuleFor(x => x.UnitCost)
                .GreaterThan(0).WithMessage("El costo unitario debe ser mayor a cero.");
        });
    }

    public class ReceivePurchaseValidator : AbstractValidator<ReceivePurchaseRequest>
    {
        public ReceivePurchaseValidator()
        {
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty().WithMessage("El producto es obligatorio.");

                item.RuleFor(x => x.ReceivedQuantity)
                    .GreaterThan(0)
                    .WithMessage("La cantidad recibida debe ser mayor a cero.");

                item.RuleFor(x => x.ActualUnitCost)
                    .GreaterThan(0)
                    .WithMessage("El costo real debe ser mayor a cero.");
            });
        }
    }
}