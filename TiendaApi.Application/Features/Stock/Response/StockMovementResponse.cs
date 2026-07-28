namespace TiendaApi.Application.Features.Stock.Response;
public class StockMovementResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }

    // Producto
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;

    // Usuario que realizó el movimiento
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}