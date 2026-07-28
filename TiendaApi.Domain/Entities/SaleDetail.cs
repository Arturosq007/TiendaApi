using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class SaleDetail : BaseEntity
{
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal { get; private set; }

    // FK
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }

    // Navegación
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;

    private SaleDetail() { }

    public static SaleDetail Create(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        if (unitPrice <= 0)
            throw new DomainException("El precio unitario debe ser mayor a cero.");

        return new SaleDetail
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = quantity * unitPrice
        };
    }
}