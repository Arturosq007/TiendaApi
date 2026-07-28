using FluentValidation;
using TiendaApi.Application.Features.Suppliers.Request;

namespace TiendaApi.Application.Features.Suppliers.Validator;

public class  SupplierRequestValidator : AbstractValidator<ISupplierRequest>
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(255).WithMessage("El correo electrónico no puede superar 255 caracteres.");
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede superar 20 caracteres.");
        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("La dirección no puede superar 500 caracteres.");
    }

    public class CreateSupplierValidator : AbstractValidator<CreateSupplierRequest>
    {
        public CreateSupplierValidator()
        {
            Include(new SupplierRequestValidator());
        }
    }

    public class UpdateSupplierValidator : AbstractValidator<UpdateSupplierRequest>
    {
        public UpdateSupplierValidator()
        {
            Include(new SupplierRequestValidator());
        }
    }
}