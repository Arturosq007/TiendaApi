namespace TiendaApi.Application.Features.Customers.Response;
public class CustomerResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public decimal? AvailableCredit { get; set; }
    public bool HasDebt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CustomerDebtResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public decimal? AvailableCredit { get; set; }

    // Últimas ventas al fiado sin pagar
    public List<PendingSaleResponse> PendingSales { get; set; } = [];
}

public class PendingSaleResponse
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
}