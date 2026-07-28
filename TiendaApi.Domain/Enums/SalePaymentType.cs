namespace TiendaApi.Domain.Enums;
public enum SalePaymentType
{
    Efectivo = 1,
    Tarjeta = 2,
    Transferencia = 3,
    Fiado = 4        // Genera deuda en el cliente
}