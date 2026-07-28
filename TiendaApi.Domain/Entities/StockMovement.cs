using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class StockMovement : BaseEntity
{
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public int StockBefore { get; private set; }
    public int StockAfter { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }

    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }

    // Navegación
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;

    private StockMovement() { }
    public static StockMovement ForSale(Guid productId, Guid userId,
        int quantity, int stockBefore, Guid saleId)
    {
        ValidateQuantity(quantity);
        return new StockMovement
        {
            ProductId = productId,
            UserId = userId,
            Type = MovementType.Venta,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = stockBefore - quantity,
            ReferenceId = saleId,
            ReferenceType = "Sale"
        };
    }
    public static StockMovement ForPurchase(Guid productId, Guid userId,
        int quantity, int stockBefore, Guid purchaseId)
    {
        ValidateQuantity(quantity);
        return new StockMovement
        {
            ProductId = productId,
            UserId = userId,
            Type = MovementType.CompraProveedor,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = stockBefore + quantity,
            ReferenceId = purchaseId,
            ReferenceType = "Purchase"
        };
    }
    public static StockMovement ForManualAdjustment(Guid productId, Guid userId,
        int quantity, int stockBefore, bool isPositive, string reason)
    {
        ValidateQuantity(quantity);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("El motivo del ajuste manual es obligatorio.");

        return new StockMovement
        {
            ProductId = productId,
            UserId = userId,
            Type = isPositive ? MovementType.AjustePositivo : MovementType.AjusteNegativo,
            Quantity = Math.Abs(quantity),
            StockBefore = stockBefore,
            StockAfter = isPositive ? stockBefore + quantity : stockBefore - quantity,
            Reason = reason.Trim(),
            ReferenceType = "Adjustment"
        };
    }

    public static StockMovement ForSaleCancellation(Guid productId, Guid userId,
        int quantity, int stockBefore, Guid saleId)
    {
        ValidateQuantity(quantity);
        return new StockMovement
        {
            ProductId = productId,
            UserId = userId,
            Type = MovementType.DevolucionVenta,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = stockBefore + quantity,
            ReferenceId = saleId,
            ReferenceType = "Sale"
        };
    }
    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad del movimiento debe ser mayor a cero.");
    }
}