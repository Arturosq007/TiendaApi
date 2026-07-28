using FluentValidation;
using TiendaApi.Application.Features.Products.Request;

namespace TiendaApi.Application.Features.Products.Validator;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El precio de compra no puede ser negativo.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a cero.")
            .GreaterThanOrEqualTo(x => x.PurchasePrice)
            .WithMessage("El precio de venta no puede ser menor al de compra.");

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El stock inicial no puede ser negativo.");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El precio de compra no puede ser negativo.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a cero.")
            .GreaterThanOrEqualTo(x => x.PurchasePrice)
            .WithMessage("El precio de venta no puede ser menor al de compra.");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");
    }
}