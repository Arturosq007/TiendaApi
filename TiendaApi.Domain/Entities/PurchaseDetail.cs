using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class PurchaseDetail : BaseEntity
{
    public int Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal Subtotal { get; private set; }

    // FK
    public Guid PurchaseId { get; set; }
    public Guid ProductId { get; set; }

    // Navegación
    public Purchase Purchase { get; set; } = null!;
    public Product Product { get; set; } = null!;


    private PurchaseDetail() { }
    public static PurchaseDetail Create(Guid productId, int quantity, decimal unitCost)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        if (unitCost <= 0)
            throw new DomainException("El costo unitario debe ser mayor a cero.");

        return new PurchaseDetail
        {
            ProductId = productId,
            Quantity = quantity,
            UnitCost = unitCost,
            Subtotal = quantity * unitCost
        };
    }

}
