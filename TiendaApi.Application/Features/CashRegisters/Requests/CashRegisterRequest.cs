namespace TiendaApi.Application.Features.CashRegisters.Requests;

public class OpenCashRegisterRequest
{
    public decimal OpeningBalance { get; set; }
    public string? Notes { get; set; }
}

public class CloseCashRegisterRequest
{
    public decimal ClosingBalance { get; set; }
    public string? Notes { get; set; }
}
