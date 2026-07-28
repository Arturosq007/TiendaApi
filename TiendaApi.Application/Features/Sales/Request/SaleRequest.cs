using TiendaApi.Domain.Enums;

namespace TiendaApi.Application.Features.Sales.Request;

public record CreateSaleRequest(
    SalePaymentType PaymentType,
    Guid? CustomerId,
    Guid? CashRegisterId,
    List<SaleDetailRequest> Details
    
);
public record SaleDetailRequest(
    Guid ProductId,
    int Quantity
    //decimal UnitPrice
);