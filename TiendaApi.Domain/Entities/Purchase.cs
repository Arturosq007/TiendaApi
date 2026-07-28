using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class Purchase : BaseEntity
{
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public PurchaseStatus Status { get; private set; } = PurchaseStatus.Pendiente;
    public PurchasePaymentType PaymentType { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal IGV { get; private set; }
    public decimal Total { get; private set; }
    public string? InvoiceNumber { get; private set; }
    // FK
    public Guid? SupplierId { get; private set; }
    public Guid UserId { get; private set; }

    // Navegación
    public Supplier? Supplier { get; private set; }
    public User User { get; private set; } = null!;
    public ICollection<PurchaseDetail> Details { get; private set; } = [];

    private Purchase() { }
    public static Purchase Create(
        Guid? supplierId,
        Guid userId,
        PurchasePaymentType paymentType,
        string? invoiceNumber = null)
    {
        return new Purchase(supplierId, userId, paymentType, invoiceNumber);
    }

    private Purchase(
        Guid? supplierId,
        Guid userId,
        PurchasePaymentType paymentType,
        string? invoiceNumber)
    {

        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        SupplierId = supplierId;
        UserId = userId;
        PaymentType = paymentType;
        InvoiceNumber = invoiceNumber?.Trim();
    }


    public bool IsPending() => Status == PurchaseStatus.Pendiente;
    public bool IsReceived() => Status == PurchaseStatus.Recibida;
    public bool IsCancelled() => Status == PurchaseStatus.Cancelada;
    public bool CanBeReceived() => IsPending();
    public bool CanBeCancelled() => IsPending();


    public void AddDetails(IEnumerable<(Guid ProductId, int Quantity, decimal UnitPrice)> items)
    {
        if (IsCancelled())
            throw new InvalidEntityStateException("Compra", "Cancelada", "agregar detalles");
        if (!IsPending())
            throw new InvalidEntityStateException("Compra", Status.ToString(), "agregar detalles");
        foreach (var (ProductId, Quantity, UnitPrice) in items)
        {
            var detail = PurchaseDetail.Create(ProductId, Quantity, UnitPrice);
            Details.Add(detail);
        }

        RecalculateTotal();
    }
    public void Receive()
    {
        if (!CanBeReceived())
            throw new InvalidEntityStateException("Compra", Status.ToString(), "recibir");

        Status = PurchaseStatus.Recibida;
    }
    public void Cancel()
    {
        if (!CanBeCancelled())
            throw new InvalidEntityStateException("Compra", Status.ToString(), "cancelar");

        Status = PurchaseStatus.Cancelada;
    }
    public void UpdateInvoiceNumber(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new DomainException("El número de factura no puede estar vacío.");

        InvoiceNumber = invoiceNumber.Trim();
    }
    private void RecalculateTotal()
    {
        // 1. Obtenemos la suma directa de lo que cuesta la mercancía (Total bruto de los detalles)
        decimal totalDetalles = Details.Sum(d => d.Subtotal);

        // 2. Desglosamos basándonos en buenas prácticas contables
        Total = Math.Round(totalDetalles, 2);
        Subtotal = Math.Round(totalDetalles / 1.18m, 2);
        IGV = Math.Round(Total - Subtotal, 2);
    }
}