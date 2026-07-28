using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Domain.Entities;
public class Alert : BaseEntity
{
    public AlertType Type { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; } = false;
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }

    private Alert() { }
    private Alert(AlertType type, string message, Guid? referenceId, string? referenceType)
    {
        Type = type;
        Message = message;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
    }
    public void MarkAsRead() => IsRead = true;

    public static Alert ForLowStock(Guid productId, string productName, int currentStock)
        => new(AlertType.StockMinimo,
            $"Stock bajo: '{productName}' tiene {currentStock} unidades.",
            productId, "Product");

    public static Alert ForOutOfStock(Guid productId, string productName)
        => new(AlertType.StockAgotado,
            $"Sin stock: '{productName}' está agotado.",
            productId, "Product");

    public static Alert ForCreditLimit(Guid customerId, string customerName, decimal debt, decimal limit)
        => new(AlertType.DeudaLimiteCliente,
            $"'{customerName}' alcanzó su límite de crédito ({limit:C}). Deuda actual: {debt:C}.",
            customerId, "Customer");

    public static Alert ForPendingPurchase(Guid purchaseId, string supplierName, int daysWaiting)
        => new(AlertType.CompraPendiente,
            $"Compra a '{supplierName}' lleva {daysWaiting} días sin recibirse.",
            purchaseId, "Purchase");
}