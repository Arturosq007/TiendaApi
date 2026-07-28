namespace TiendaApi.Domain.Helpers;

public static class TicketNumberGenerator
{
    public static string Generate()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randPart = Random.Shared.Next(1000, 9999); // Genera un número aleatorio de 4 dígitos
        return $"TCK-{datePart}-{randPart}";
    }
}
