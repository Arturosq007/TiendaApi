using TiendaApi.Application.Features.Purchases.Request;
using TiendaApi.Application.Features.Purchases.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Purchases;

public class PurchaseService(IUnitOfWork _uow) : IPurchaseService
{
    public async Task<PurchaseResponse> GetByIdAsync(Guid id)
    {
        var purchase = await _uow.Purchases.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Compra", id);

        return MapToResponse(purchase);
    }
    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, Guid userId)
    {
        Supplier? supplier = null;
        if (request.SupplierId.HasValue)
        {
            supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId.Value)
                ?? throw new NotFoundException("Proveedor", request.SupplierId.Value);

            if (!supplier.IsActive)
                throw new DomainException(
                    $"El proveedor '{supplier.Name}' está inactivo.");
        }
        // Traer todos los productos en una sola query
        var productIds = request.Items.Select(d => d.ProductId).Distinct().ToList();
        var products = await _uow.Products.GetByIdsAsync(productIds);
        var productDict = products.ToDictionary(p => p.Id);

        foreach (var item in request.Items)
        {
            if (!productDict.TryGetValue(item.ProductId, out var product))
                throw new NotFoundException("Producto", item.ProductId);

            if (!product.IsActive)
                throw new DomainException(
                    $"El producto '{product.Name}' no está activo.");
        }

        // Crear la compra
        var purchase = Purchase.Create(
            supplierId: supplier?.Id,
            userId: userId,
            paymentType: request.PaymentType,
            invoiceNumber: request.InvoiceNumber);

        // Agregar detalles con el costo del request, no el precio de venta
        var details = request.Items
            .Select(d => (d.ProductId, d.Quantity, d.UnitCost))
            .ToList();

        purchase.AddDetails(details);

        await _uow.Purchases.AddAsync(purchase);
        await _uow.SaveChangesAsync();

        var created = await _uow.Purchases.GetWithDetailsAsync(purchase.Id);
        return MapToResponse(created!);
    }

    public async Task<PurchaseResponse> ReceiveAsync(Guid id, ReceivePurchaseRequest request, Guid userId)
    {
        PurchaseResponse response = null!;

        await _uow.ExecuteTransactionAsync(async () =>
        {
            var purchase = await _uow.Purchases.GetWithDetailsAsync(id)
                ?? throw new NotFoundException("Compra", id);

            purchase.Receive();

            if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
                purchase.UpdateInvoiceNumber(request.InvoiceNumber);

            var itemsToProcess = request.Items.Count != 0
                ? request.Items
                : [.. purchase.Details.Select(d => new ReceivePurchaseItemRequest
            {
                ProductId = d.ProductId,
                ReceivedQuantity = d.Quantity,
                ActualUnitCost = d.UnitCost
            })];

            var productIds = itemsToProcess.Select(i => i.ProductId).Distinct().ToList();
            var products = await _uow.Products.GetByIdsAsync(productIds);
            var productDict = products.ToDictionary(p => p.Id);

            foreach (var item in itemsToProcess)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product))
                    throw new NotFoundException("Producto", item.ProductId);

                var stockBefore = product.Stock;
                product.AddStock(item.ReceivedQuantity);

                product.Update(
                    name: product.Name,
                    purchasePrice: item.ActualUnitCost,
                    salePrice: product.SalePrice,
                    minStock: product.MinStock,
                    categoryId: product.CategoryId,
                    supplierId: product.SupplierId);

                _uow.Products.Update(product);

                var movement = StockMovement.ForPurchase(
                    productId: product.Id,
                    userId: userId,
                    quantity: item.ReceivedQuantity,
                    stockBefore: stockBefore,
                    purchaseId: purchase.Id);

                await _uow.StockMovements.AddAsync(movement);
            }

            _uow.Purchases.Update(purchase);
            await _uow.SaveChangesAsync();

            var received = await _uow.Purchases.GetWithDetailsAsync(purchase.Id);
            response = MapToResponse(received!);
        });

        return response;
    }

    public async Task<PagedResult<PurchaseSummaryResponse>> GetPagedAsync(int page, int pageSize, 
        Guid? supplierId = null, PurchaseStatus? status = null, DateTime? from = null, DateTime? to = null)
    {
        var result = await _uow.Purchases.GetPagedAsync(page, pageSize, supplierId, status, from, to);
        var dtos = result.Items.Select(MapToSummary).ToList();

        return new PagedResult<PurchaseSummaryResponse>(dtos, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task CancelAsync(Guid id, Guid userId)
    {
        var purchase = await _uow.Purchases.GetByIdAsync(id)
            ?? throw new NotFoundException("Compra", id);

        // La entidad valida que esté en estado Pendiente
        purchase.Cancel();

        _uow.Purchases.Update(purchase);
        await _uow.SaveChangesAsync();
    }

    private static PurchaseResponse MapToResponse(Purchase purchase)
    {
        return new PurchaseResponse
        {
            Id = purchase.Id,
            Date = purchase.Date,
            Status = purchase.Status switch
            {
                PurchaseStatus.Pendiente => "Pendiente",
                PurchaseStatus.Recibida => "Recibida",
                PurchaseStatus.Cancelada => "Cancelada",
                _ => "Desconocido"
            },
            PaymentType = purchase.PaymentType switch
            {
                PurchasePaymentType.Contado => "Efectivo",
                PurchasePaymentType.Credito => "Crédito",
                _ => "Desconocido"
            },
            Subtotal = purchase.Subtotal,
            IGV = purchase.IGV,
            Total = purchase.Total,
            InvoiceNumber = purchase.InvoiceNumber,
            SupplierId = purchase.SupplierId,
            SupplierName = purchase.Supplier?.Name ?? string.Empty,
            UserId = purchase.UserId,
            UserFullName = purchase.User.FullName ?? string.Empty,
            Details = [.. purchase.Details.Select(d => new PurchaseDetailResponse
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product.Name,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                Subtotal = d.Subtotal,
                ProductSKU = d.Product.SKU
            })],
            CreatedAt = purchase.CreatedAt,
            UpdatedAt = purchase.UpdatedAt,
        };
    }
    private static PurchaseSummaryResponse MapToSummary(Purchase purchase)
    {
        return new PurchaseSummaryResponse
        {
            Id = purchase.Id,
            Date = purchase.Date,
            Status = purchase.Status switch
            {
                PurchaseStatus.Pendiente => "Pendiente",
                PurchaseStatus.Recibida => "Recibida",
                PurchaseStatus.Cancelada => "Cancelada",
                _ => "Desconocido"
            },
            PaymentType = purchase.PaymentType switch
            {
                PurchasePaymentType.Contado => "Efectivo",
                PurchasePaymentType.Credito => "Crédito",
                _ => "Desconocido"
            },
            Total = purchase.Total,
            InvoiceNumber = purchase.InvoiceNumber ?? string.Empty,
            SupplierName = purchase.Supplier?.Name ?? string.Empty,
            UserFullName = purchase.User.FullName ?? string.Empty,
        };
    }
}