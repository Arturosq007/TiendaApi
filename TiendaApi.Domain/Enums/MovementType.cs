namespace TiendaApi.Domain.Enums;
public enum MovementType
{
    // Entradas de stock
    CompraProveedor = 1,    // Por una compra registrada
    AjustePositivo = 2,     // Corrección manual hacia arriba

    // Salidas de stock
    Venta = 3,              // Por una venta procesada
    AjusteNegativo = 4,     // Corrección manual hacia abajo

    // Reversiones
    DevolucionVenta = 5,    // Cancelación de venta — devuelve stock
    DevolucionCompra = 6    // Cancelación de compra — quita stock
}