namespace TiendaApi.Application.Features.Products.Response;
public record ProductResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal PurchasePrice { get; init; }
    public decimal SalePrice { get; init; }
    public decimal Margin { get; init; }
    public decimal MarginPercent { get; init; }
    public int Stock { get; init; }
    public int MinStock { get; init; }
    public bool IsActive { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }

    // Datos de la categoría y proveedor
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid? SupplierId { get; init; }
    public string? SupplierName { get; init; } // Sin asignar string.Empty por ser opcional

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
