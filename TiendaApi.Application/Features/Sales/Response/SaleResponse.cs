namespace TiendaApi.Application.Features.Sales.Response;

public record SaleResponse
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal IGV { get; set; }
    public decimal Total { get; set; }
    // Cliente — null si la venta fue sin cliente registrado
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    // Cajero que procesó la venta
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;

    public List<SaleDetailResponse> Details { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public record SaleDetailResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public record SaleSummaryResponse
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public bool IsCancelled { get; set; }
    public string? CustomerName { get; set; }
    public string UserFullName { get; set; } = string.Empty;
}