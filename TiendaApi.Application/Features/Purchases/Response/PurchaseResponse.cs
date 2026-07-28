namespace TiendaApi.Application.Features.Purchases.Response;

public record PurchaseResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public decimal IGV { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    // Usuario que procesó la compra
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public List<PurchaseDetailResponse> Details { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record PurchaseDetailResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost{ get; set; }
    public decimal Subtotal { get; set; }
}

public record PurchaseSummaryResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? SupplierName { get; set; }
    public string UserFullName { get; set; } = string.Empty;
}