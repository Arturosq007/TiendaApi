namespace TiendaApi.Application.Features.Purchases.Request;

public class ReceivePurchaseRequest
{
    // Permite actualizar el número de factura al recibir
    // si no se tenía al crear la compra
    public string? InvoiceNumber { get; set; }

    // Permite ajustar los costos reales al recibir
    // por si hubo cambio de precio respecto a lo pactado
    public List<ReceivePurchaseItemRequest> Items { get; set; } = [];
}

public class ReceivePurchaseItemRequest
{
    public Guid ProductId { get; set; }

    // Cantidad realmente recibida — puede diferir de lo pedido
    public int ReceivedQuantity { get; set; }

    // Costo real — puede diferir del pactado
    public decimal ActualUnitCost { get; set; }
}