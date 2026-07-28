using TiendaApi.Application.Features.Sales.Request;
using TiendaApi.Application.Features.Sales.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Sales;

public class SaleService(IUnitOfWork _uow) : ISaleService
{
    public async Task<SaleResponse> GetByIdAsync(Guid id)
    {
        var sale = await _uow.Sales.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Venta", id);

        return MapToDto(sale);
    }

    public async Task<PagedResult<SaleSummaryResponse>> GetPagedAsync(
        int page, int pageSize,
        DateTime? from = null,
        DateTime? to = null,
        Guid? customerId = null,
        Guid? userId = null,
        bool? isCancelled = null)
    {
        var result = await _uow.Sales.GetPagedAsync(
            page, pageSize, from, to,
            customerId, userId, null, isCancelled);

        var dtos = result.Items.Select(MapToSummaryDto).ToList();

        return new PagedResult<SaleSummaryResponse>(
            dtos, result.TotalCount,
            result.Page, result.PageSize);
    }

    public async Task<SaleResponse> CreateAsync(CreateSaleRequest request, Guid userId)
    {
        SaleResponse response = null!;

        await _uow.ExecuteTransactionAsync(async () =>
        {
            // 1 — Validar cliente si es fiado
            Customer? customer = null;

            if (request.PaymentType == SalePaymentType.Fiado)
            {
                if (!request.CustomerId.HasValue)
                    throw new DomainException("El cliente es obligatorio para ventas al fiado.");

                customer = await _uow.Customers.GetByIdAsync(request.CustomerId.Value)
                    ?? throw new NotFoundException("Cliente", request.CustomerId.Value);

                if (!customer.IsActive)
                    throw new DomainException($"El cliente '{customer.FullName}' está inactivo.");
            }

            // 2 — Cargar productos en una sola consulta
            var productIds = request.Details.Select(d => d.ProductId).Distinct().ToList();
            var products = await _uow.Products.GetByIdsAsync(productIds);
            var productDict = products.ToDictionary(p => p.Id);

            // 3 — Crear la venta
            var sale = Sale.Create(
                userId: userId,
                paymentType: request.PaymentType,
                cashRegisterId: request.CashRegisterId,
                customerId: request.CustomerId
            );

            var detailsToDomain = new List<(Guid ProductId, int Quantity, decimal UnitPrice)>();
            var alertsToCreate = new List<Alert>();

            // 4 — Procesar cada ítem
            foreach (var item in request.Details)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product))
                    throw new NotFoundException("Producto", item.ProductId);

                if (!product.IsActive)
                    throw new DomainException($"El producto '{product.Name}' no está disponible.");

                detailsToDomain.Add((product.Id, item.Quantity, product.SalePrice));

                var stockBefore = product.Stock;
                product.DeductStock(item.Quantity);
                _uow.Products.Update(product);

                var movement = StockMovement.ForSale(
                    productId: product.Id,
                    userId: userId,
                    quantity: item.Quantity,
                    stockBefore: stockBefore,
                    saleId: sale.Id);

                await _uow.StockMovements.AddAsync(movement);

                var alert = await EvaluateProductStockAlertAsync(product);
                if (alert != null) alertsToCreate.Add(alert);
            }

            sale.AddDetails(detailsToDomain);

            // 5 — Si es fiado, registrar deuda
            if (request.PaymentType == SalePaymentType.Fiado && customer is not null)
            {
                customer.AddDebt(sale.Total);
                _uow.Customers.Update(customer);

                if (customer.CreditLimit > 0 && customer.CurrentDebt >= customer.CreditLimit * 0.9m)
                {
                    alertsToCreate.Add(Alert.ForCreditLimit(
                        customer.Id, customer.FullName, customer.CurrentDebt, customer.CreditLimit));
                }
            }

            if (alertsToCreate.Count > 0)
            {
                await _uow.Alerts.AddRangeAsync(alertsToCreate);
            }

            await _uow.Sales.AddAsync(sale);
            await _uow.SaveChangesAsync();

            var created = await _uow.Sales.GetWithDetailsAsync(sale.Id);
            response = MapToDto(created!);
        });

        return response;
    }

    public async Task CancelAsync(Guid id, Guid userId)
    {
        await _uow.ExecuteTransactionAsync(async () =>
        {
            var sale = await _uow.Sales.GetWithDetailsAsync(id)
                ?? throw new NotFoundException("Venta", id);

            sale.Cancel();

            var productIds = sale.Details.Select(d => d.ProductId).Distinct().ToList();
            var products = await _uow.Products.GetByIdsAsync(productIds);
            var productDict = products.ToDictionary(p => p.Id);

            foreach (var detail in sale.Details)
            {
                if (!productDict.TryGetValue(detail.ProductId, out var product))
                    throw new NotFoundException("Producto", detail.ProductId);

                var stockBefore = product.Stock;
                product.AddStock(detail.Quantity);
                _uow.Products.Update(product);

                var movement = StockMovement.ForSaleCancellation(
                    productId: product.Id,
                    userId: userId,
                    quantity: detail.Quantity,
                    stockBefore: stockBefore,
                    saleId: sale.Id);

                await _uow.StockMovements.AddAsync(movement);
            }

            if (sale.IsCredit() && sale.CustomerId.HasValue)
            {
                var customer = await _uow.Customers.GetByIdAsync(sale.CustomerId.Value);
                if (customer is not null)
                {
                    customer.DecreaseDebt(sale.Total);
                    _uow.Customers.Update(customer);
                }
            }

            _uow.Sales.Update(sale);
            await _uow.SaveChangesAsync();
        });
    }

    public async Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _uow.Sales.GetTotalByDateRangeAsync(from, to);
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────

    private async Task<Alert?> EvaluateProductStockAlertAsync(Product product)
    {
        if (product.IsOutOfStock())
        {
            var alertExists = await _uow.Alerts.ExistsAsync(a =>
                a.ReferenceId == product.Id && a.Type == AlertType.StockAgotado && !a.IsRead);

            if (!alertExists)
                return Alert.ForOutOfStock(product.Id, product.Name);
        }
        else if (product.IsLowStock())
        {
            var alertExists = await _uow.Alerts.ExistsAsync(a =>
                a.ReferenceId == product.Id && a.Type == AlertType.StockMinimo && !a.IsRead);

            if (!alertExists)
                return Alert.ForLowStock(product.Id, product.Name, product.Stock);
        }

        return null;
    }

    // ─── Mapeo ────────────────────────────────────────────────────────────────

    private static SaleResponse MapToDto(Sale sale) => new()
    {
        Id = sale.Id,
        TicketNumber = sale.TicketNumber,
        Date = sale.Date,
        Status = sale.Status.ToString(),
        PaymentType = sale.PaymentType.ToString(),
        Subtotal = sale.Subtotal,
        IGV = sale.IGV,
        Total = sale.Total,
        CustomerId = sale.CustomerId,
        CustomerName = sale.Customer?.FullName,
        UserId = sale.UserId,
        UserFullName = sale.User?.FullName ?? string.Empty,
        Details = [.. sale.Details.Select(d => new SaleDetailResponse
        {
            Id = d.Id,
            ProductId = d.ProductId,
            ProductName = d.Product?.Name ?? string.Empty,
            ProductSKU = d.Product?.SKU ?? string.Empty,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            Subtotal = d.Subtotal
        })],
        CreatedAt = sale.CreatedAt
    };

    private static SaleSummaryResponse MapToSummaryDto(Sale sale) => new()
    {
        Id = sale.Id,
        TicketNumber = sale.TicketNumber,
        Date = sale.Date,
        Status = sale.Status.ToString(),
        PaymentType = sale.PaymentType.ToString(),
        Total = sale.Total,
        CustomerName = sale.Customer?.FullName,
        UserFullName = sale.User?.FullName ?? string.Empty
    };
}