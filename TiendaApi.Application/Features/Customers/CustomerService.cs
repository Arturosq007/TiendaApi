using TiendaApi.Application.Features.Customers.Request;
using TiendaApi.Application.Features.Customers.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Customers;

public class CustomerService(IUnitOfWork _uow) : ICustomerService
{

    public async Task<CustomerResponse> GetByIdAsync(Guid id)
    {
        var customer = await _uow.Customers.GetByIdAsync(id)
            ?? throw new NotFoundException("Cliente", id);

        return MapToDto(customer);
    }

    public async Task<PagedResultWithStatus<CustomerResponse>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? hasDebt = null,
        bool? isActive = null)
    {
        var result = await _uow.Customers.GetPagedAsync(
            page, pageSize, search, hasDebt, isActive);

        var dtos = result.Items.Select(MapToDto).ToList();

        return new PagedResultWithStatus<CustomerResponse>(
            dtos, result.TotalCount, result.Page,
            result.PageSize, result.ActivesCount,
            result.InactivesCount);
    }

    public async Task<IReadOnlyCollection<CustomerDebtResponse>> GetWithDebtAsync()
    {
        // Una sola consulta a la BD que nos trae la estructura optimizada en tuplas
        var customerData = await _uow.Customers.GetCustomersWithTopPendingSalesAsync();

        // Mapeamos las tuplas al DTO final en memoria
        return customerData.Select(data => new CustomerDebtResponse
        {
            Id = data.Customer.Id,
            FullName = data.Customer.FullName,
            Phone = data.Customer.Phone,
            CreditLimit = data.Customer.CreditLimit,
            CurrentDebt = data.Customer.CurrentDebt,
            AvailableCredit = data.Customer.GetAvailableCredit(), // Usamos el método de negocio de la entidad
            PendingSales = data.TopSales.Select(s => new PendingSaleResponse
            {
                Id = s.Id,
                TicketNumber = s.TicketNumber,
                Date = s.Date,
                Total = s.Total
            }).ToList()
        }).ToList();
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        var customer = Customer.Create(
            fullName: request.FullName,
            phone: request.Phone,
            address: request.Address,
            creditLimit: request.CreditLimit);

        await _uow.Customers.AddAsync(customer);
        await _uow.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _uow.Customers.GetByIdAsync(id)
            ?? throw new NotFoundException("Cliente", id);

        customer.Update(
            fullName: request.FullName,
            phone: request.Phone,
            address: request.Address,
            creditLimit: request.CreditLimit);

        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        var customer = await _uow.Customers.GetByIdAsync(id)
            ?? throw new NotFoundException("Cliente", id);

        // No se puede desactivar un cliente con deuda pendiente
        if (customer.IsActive && customer.HasDebt())
            throw new DomainException(
                $"No se puede desactivar a '{customer.FullName}' " +
                $"porque tiene una deuda pendiente de {customer.CurrentDebt:C}.");

        if (customer.IsActive)
            customer.Deactivate();
        else
            customer.Activate();

        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync();
    }

    public async Task RegisterPaymentAsync(
        Guid customerId,
        RegisterPaymentRequest request,
        Guid userId)
    {
        var customer = await _uow.Customers.GetByIdAsync(customerId)
            ?? throw new NotFoundException("Cliente", customerId);

        if (!customer.HasDebt())
            throw new DomainException(
                $"El cliente '{customer.FullName}' no tiene deuda pendiente.");

        // La entidad valida que el pago no supere la deuda
        customer.RegisterPayment(request.Amount);

        // Crear el registro del pago
        var payment = CreditPayment.Create(
            customerId: customerId,
            userId: userId,
            amount: request.Amount,
            notes: request.Notes);

        _uow.Customers.Update(customer);
        await _uow.CreditPayments.AddAsync(payment);
        await _uow.SaveChangesAsync();
    }

    // ─── Mapeo ────────────────────────────────────────────────────────────────

    private static CustomerResponse MapToDto(Customer customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Phone = customer.Phone,
        Address = customer.Address,
        CreditLimit = customer.CreditLimit,
        CurrentDebt = customer.CurrentDebt,
        AvailableCredit = customer.GetAvailableCredit(),
        HasDebt = customer.HasDebt(),
        IsActive = customer.IsActive,
        CreatedAt = customer.CreatedAt,
        UpdatedAt = customer.UpdatedAt
    };
}