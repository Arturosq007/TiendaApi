namespace TiendaApi.Application.Features.Suppliers.Response;

public record SupplierResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    string? ContactEmail = null,
    string? Phone = null,
    string? Address = null,
    DateTime? UpdatedAt = null
);