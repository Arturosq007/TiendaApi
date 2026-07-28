using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class Product : ActivatableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal PurchasePrice { get; private set; }
    public decimal SalePrice { get; private set; }
    public int Stock { get; private set; }
    public int MinStock { get; private set; } = 5;

    // FK
    public Guid CategoryId { get; private set; }
    public Guid? SupplierId { get; private set; }

    // Navegación
    public Category Category { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<SaleDetail> SaleDetails { get; private set; } = [];
    public ICollection<PurchaseDetail> PurchaseDetails { get; private set; } = [];
    public ICollection<StockMovement> StockMovements { get; private set; } = [];

    // ─── Propiedades calculadas 
    public decimal Margin => SalePrice - PurchasePrice;
    private Product() { }
    private Product(string name,string sku, decimal purchasePrice, decimal salePrice, int minStock, Guid categoryId, Guid? supplierId, int stock = 0)
    {
        if (stock < 0)
            throw new DomainException("El stock inicial no puede ser negativo.");
        Stock = stock;
        SKU = sku;
        ApplyValues(name, purchasePrice, salePrice, minStock, categoryId, supplierId);
    }
    public static Product Create(string name, string sku, decimal purchasePrice,
        decimal salePrice, int minStock, Guid categoryId, Guid? supplierId = null, int stock = 0)
    {
        return new Product(name, sku, purchasePrice, salePrice, minStock, categoryId, supplierId, stock);
    }
    public void Update(string name, decimal purchasePrice, decimal salePrice,
        int minStock, Guid categoryId, Guid? supplierId)
    {
        ApplyValues(name, purchasePrice, salePrice, minStock, categoryId, supplierId);
    }
    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad a descontar debe ser mayor a cero.");
        if (Stock < quantity)
            throw new InsufficientStockException(Name, Stock, quantity);

        Stock -= quantity;
    }
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad a agregar debe ser mayor a cero.");

        Stock += quantity;
    }
    public bool IsLowStock() => Stock <= MinStock;
    public bool IsOutOfStock() => Stock <= 0;
    public decimal GetMarginPercent()
    {
        if (PurchasePrice == 0) return 0;
        return Math.Round((SalePrice - PurchasePrice) / PurchasePrice * 100, 2);
    }

    private void ApplyValues(string name, decimal purchasePrice, decimal salePrice,
        int minStock, Guid categoryId, Guid? supplierId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre es obligatorio.");

        if (purchasePrice < 0)
            throw new DomainException("El precio de compra no puede ser negativo.");

        if (salePrice < purchasePrice)
            throw new DomainException("El precio de venta no puede ser menor al de compra.");

        if (minStock < 0)
            throw new DomainException("El stock mínimo no puede ser negativo.");

        Name = name.Trim();
        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
        MinStock = minStock;
        CategoryId = categoryId;
        SupplierId = supplierId;
    }
}