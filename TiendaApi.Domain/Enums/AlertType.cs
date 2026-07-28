namespace TiendaApi.Domain.Enums;
public enum AlertType
{
    StockMinimo = 1,         // Producto llegó al stock mínimo
    StockAgotado = 2,        // Producto sin stock
    DeudaLimiteCliente = 3,  // Cliente superó su límite de crédito
    CompraPendiente = 4      // Compra lleva mucho tiempo sin recibirse
}

