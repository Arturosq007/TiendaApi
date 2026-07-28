namespace TiendaApi.Application.Features.Suppliers.Request;

public interface ISupplierRequest
{
    string Name { get; }
    string? ContactEmail { get; }
    string? Phone { get; }
    string? Address { get; }
}
public record CreateSupplierRequest(string Name, string? ContactEmail = null, string? Phone = null, string? Address = null) : ISupplierRequest;

public record UpdateSupplierRequest(string Name, string? ContactEmail = null, string? Phone = null, string? Address = null) : ISupplierRequest;