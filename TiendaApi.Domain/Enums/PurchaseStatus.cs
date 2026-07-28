namespace TiendaApi.Domain.Enums;
public enum PurchaseStatus
{
    Pendiente = 1,   // Creada, esperando recepción física
    Recibida = 2,    // Mercadería ingresada al stock
    Cancelada = 3    // Anulada antes de recibir
}
