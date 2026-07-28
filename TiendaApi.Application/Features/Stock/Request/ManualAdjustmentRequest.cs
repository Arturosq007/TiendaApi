namespace TiendaApi.Application.Features.Stock.Request;
public class ManualAdjustmentRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsPositive { get; set; }  // true = entrada, false = salida
    public string Reason { get; set; } = string.Empty;
}