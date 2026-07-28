using FluentValidation;
using TiendaApi.Application.Features.Stock.Request;

namespace TiendaApi.Application.Features.Stock.Validation;

public class ManualAdjustmentValidator : AbstractValidator<ManualAdjustmentRequest>
{
    public ManualAdjustmentValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("El producto es obligatorio.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo del ajuste es obligatorio.")
            .MaximumLength(500).WithMessage("El motivo no puede superar 500 caracteres.");
    }
}