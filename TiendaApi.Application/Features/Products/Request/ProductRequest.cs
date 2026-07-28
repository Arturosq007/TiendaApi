namespace TiendaApi.Application.Features.Products.Request;
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int InitialStock { get; set; }
    public int MinStock { get; set; } = 5;
    public Guid CategoryId { get; set; }
    public Guid? SupplierId { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int MinStock { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? SupplierId { get; set; }
}