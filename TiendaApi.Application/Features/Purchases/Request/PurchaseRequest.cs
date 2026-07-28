using TiendaApi.Domain.Enums;

namespace TiendaApi.Application.Features.Purchases.Request;

public record CreatePurchaseRequest(
    PurchasePaymentType PaymentType,
    Guid? SupplierId,
    string? InvoiceNumber,
    //Guid? CashRegisterId,
    List<PurchaseItemRequest> Items
);

public record PurchaseItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitCost
);