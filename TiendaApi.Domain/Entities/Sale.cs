using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Helpers;

namespace TiendaApi.Domain.Entities;
public class Sale : BaseEntity
{
    public string TicketNumber { get; private set; } = string.Empty;
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public SalePaymentType PaymentType { get; private set; }
    public SaleStatus Status { get; private set; } = SaleStatus.Completada;
    public decimal Subtotal { get; private set; }
    public decimal IGV { get; private set; }
    public decimal Total { get; private set; }

    // FK
    public Guid? CustomerId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? CashRegisterId { get; private set; }

    // Navegación
    public Customer? Customer { get; set; }
    public User User { get; set; } = null!;
    public CashRegister? CashRegister { get; set; }
    public ICollection<SaleDetail> Details { get; private set; } = [];

    private Sale() { }
    public static Sale Create(Guid userId, SalePaymentType paymentType, Guid? cashRegisterId = null, Guid? customerId = null)
    {
        return new Sale(userId, paymentType, customerId, cashRegisterId);
    }
    private Sale(Guid userId, SalePaymentType paymentType, Guid? customerId, Guid? cashRegisterId)
    {
        UserId = userId;
        PaymentType = paymentType;
        CustomerId = customerId;
        TicketNumber = TicketNumberGenerator.Generate();
        CashRegisterId = cashRegisterId;
        Date = DateTime.UtcNow;
    }

    public bool IsCredit() => PaymentType == SalePaymentType.Fiado;
    public bool IsCancelled() => Status == SaleStatus.Cancelada;
    public bool IsCompleted() => Status == SaleStatus.Completada;

    public void AddDetails(IEnumerable<(Guid ProductId, int Quantity, decimal UnitPrice)> items)
    {
        if (IsCancelled())
            throw new InvalidEntityStateException("Venta", "Cancelada", "agregar detalles");

        foreach (var (ProductId, Quantity, UnitPrice) in items)
        {
            var detail = SaleDetail.Create(ProductId, Quantity, UnitPrice);
            Details.Add(detail);
        }

        RecalculateTotals();
    }

    public void Cancel()
    {
        if (IsCancelled())
            throw new InvalidEntityStateException(
                "Venta", Status.ToString(), "cancelar");

        Status = SaleStatus.Cancelada;
    }

    private void RecalculateTotals()
    {
        // 1. Obtenemos la suma directa de lo que cuesta la mercancía (Total bruto de los detalles)
        decimal totalDetalles = Details.Sum(d => d.Subtotal);

        // 2. Desglosamos basándonos en buenas prácticas contables
        Total = Math.Round(totalDetalles, 2);
        Subtotal = Math.Round(totalDetalles / 1.18m, 2);
        IGV = Math.Round(Total - Subtotal, 2);
    }
}
