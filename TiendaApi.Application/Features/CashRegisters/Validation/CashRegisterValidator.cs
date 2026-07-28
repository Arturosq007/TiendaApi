using FluentValidation;
using TiendaApi.Application.Features.CashRegisters.Requests;

namespace TiendaApi.Application.Features.CashRegisters.Validation;

public class OpenCashRegisterValidator : AbstractValidator<OpenCashRegisterRequest>
{
    public OpenCashRegisterValidator()
    {
        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto inicial no puede ser negativo.");
    }
}

public class CloseCashRegisterValidator : AbstractValidator<CloseCashRegisterRequest>
{
    public CloseCashRegisterValidator()
    {
        RuleFor(x => x.ClosingBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El monto de cierre no puede ser negativo.");
    }
}