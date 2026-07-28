using FluentValidation;
using TiendaApi.Application.Features.Categories.Request;

namespace TiendaApi.Application.Features.Categories.Validator;

// Creamos un único validador para la interfaz
public class CategoryRequestValidator : AbstractValidator<ICategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres.");
    }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        Include(new CategoryRequestValidator()); 
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        Include(new CategoryRequestValidator()); 
    }
}
