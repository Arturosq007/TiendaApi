using TiendaApi.Application.Features.Suppliers.Request;
using TiendaApi.Application.Features.Suppliers.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Suppliers;

public class SupplierService(IUnitOfWork _uow) : ISupplierService
{
    public async Task<SupplierResponse> CreateAsync(CreateSupplierRequest request)
    {
        var existingSupplier = await _uow.Suppliers.NameExistsAsync(request.Name);
        if (existingSupplier)
        {
            throw new InvalidOperationException("A supplier with the same name already exists.");
        }

        var supplier = Supplier.Create(request.Name, request.ContactEmail, request.Phone, request.Address);
        await _uow.Suppliers.AddAsync(supplier);
        await _uow.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task<SupplierResponse> GetByIdAsync(Guid id)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(id) 
            ?? throw new NotFoundException("Supplier", id);
        return MapToDto(supplier);
    }

    public async Task<PagedResultWithStatus<SupplierResponse>> GetPagedAsync(int page, int pageSize, string? search, bool? isActive)
    {
        var result = await _uow.Suppliers.GetPagedAsync(page, pageSize, search, isActive);
        var dtos = result.Items.Select(MapToDto).ToList();

        return new PagedResultWithStatus<SupplierResponse>(
            dtos,
            result.TotalCount,
            result.Page,
            result.PageSize,
            result.ActivesCount,
            result.InactivesCount
        );  
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(id)
            ?? throw new NotFoundException("Supplier", id);

        if (supplier.IsActive)
        {
            var hasProducts = await _uow.Suppliers.HasProductsAsync(id);
            if (hasProducts)
                throw new DomainException("No se puede desactivar el proveedor porque tiene productos asociados.");
            supplier.Deactivate();
        }
        else
            supplier.Activate();
        _uow.Suppliers.Update(supplier);
        await _uow.SaveChangesAsync();
    }

    public async Task<SupplierResponse> UpdateAsync(Guid id, UpdateSupplierRequest request)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(id)
            ?? throw new NotFoundException("Supplier", id);
        var existingSupplier = await _uow.Suppliers.NameExistsAsync(request.Name, id);
        if (existingSupplier)
        {
            throw new InvalidOperationException("Un proveedor con el mismo nombre ya existe.");
        }

        supplier.Update(request.Name, request.ContactEmail, request.Phone, request.Address);
        _uow.Suppliers.Update(supplier);
        await _uow.SaveChangesAsync();

        return MapToDto(supplier);
    }

    private static SupplierResponse MapToDto(Supplier supplier)
    {
        return new SupplierResponse(
                Id: supplier.Id,
                Name: supplier.Name,
                ContactEmail: supplier.ContactEmail,
                Phone: supplier.Phone,
                Address: supplier.Address,
                CreatedAt: supplier.CreatedAt,
                UpdatedAt: supplier.UpdatedAt
            );
    }
} 