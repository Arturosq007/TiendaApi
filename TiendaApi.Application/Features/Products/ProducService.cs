using TiendaApi.Application.Features.Products.Request;
using TiendaApi.Application.Features.Products.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Helpers;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Products;

public class ProductService(IUnitOfWork _uow) : IProductService
{
    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await _uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException("Producto", id);

        return MapToDto(product);
    }

    public async Task<PagedResultWithStatus<ProductResponse>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        Guid? categoryId = null,
        Guid? supplierId = null,
        bool? isActive = null,
        bool? lowStock = null)
    {
        var result = await _uow.Products.GetPagedAsync(
            page, pageSize, search, categoryId,
            supplierId, isActive, lowStock);

        var dtos = result.Items.Select(MapToDto).ToList();

        return new PagedResultWithStatus<ProductResponse>(
            dtos, result.TotalCount, result.Page,
            result.PageSize, result.ActivesCount,
            result.InactivesCount);
    }

    // se puede eliminar si se prefiere usar solo el paginado con filtro de stock bajo
    public async Task<IReadOnlyCollection<ProductResponse>> GetLowStockAsync()
    {
        var products = await _uow.Products.GetLowStockProductsAsync();
        return [.. products.Select(MapToDto)]; // retorna una nueva lista para evitar problemas de referencia con la entidad
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request, Guid userId)
    {
        // 1 — validar que la categoría existe
        var categoryExists = await _uow.Categories
            .ExistsAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
            throw new NotFoundException("Categoría", request.CategoryId);

        // 2 — validar que el proveedor existe si se especificó
        if (request.SupplierId.HasValue)
        {
            var supplierExists = await _uow.Suppliers
                .ExistsAsync(s => s.Id == request.SupplierId.Value);

            if (!supplierExists)
                throw new NotFoundException("Proveedor", request.SupplierId.Value);
        }

        // 3 — generar SKU único
        var sequence = await _uow.Products
            .CountAsync(p => p.CategoryId == request.CategoryId) + 1;

        var category = await _uow.Categories.GetByIdAsync(request.CategoryId);

        var sku = SkuGenerator.Generate(
            category!.Name, request.Name, sequence);

        // 4 — verificar que el SKU no exista (por si acaso)
        var skuExists = await _uow.Products.SkuExistsAsync(sku);
        if (skuExists)
            sku = $"{sku}-{Guid.NewGuid().ToString()[..4]}";

        // 5 — crear el producto
        var product = Product.Create(
            name: request.Name,
            sku: sku,
            purchasePrice: request.PurchasePrice,
            salePrice: request.SalePrice,
            stock: request.InitialStock,
            minStock: request.MinStock,
            categoryId: request.CategoryId,
            supplierId: request.SupplierId
        );

        await _uow.Products.AddAsync(product);

        // 6 — si tiene stock inicial, registrar movimiento
        if (request.InitialStock > 0)
        {
            var movement = StockMovement.ForManualAdjustment(
                productId: product.Id,
                userId: userId,
                quantity: request.InitialStock,
                stockBefore: 0,
                isPositive: true,
                reason: "Stock inicial al crear el producto."
            );

            await _uow.StockMovements.AddAsync(movement);
        }

        await _uow.SaveChangesAsync();

        // 7 — recargar con relaciones para el DTO
        var created = await _uow.Products.GetByIdAsync(product.Id);
        return MapToDto(created!);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException("Producto", id);

        // Validar categoría
        var categoryExists = await _uow.Categories
            .ExistsAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
            throw new NotFoundException("Categoría", request.CategoryId);

        // Validar proveedor
        if (request.SupplierId.HasValue)
        {
            var supplierExists = await _uow.Suppliers
                .ExistsAsync(s => s.Id == request.SupplierId.Value);

            if (!supplierExists)
                throw new NotFoundException("Proveedor", request.SupplierId.Value);
        }

        product.Update(
            name: request.Name,
            purchasePrice: request.PurchasePrice,
            salePrice: request.SalePrice,
            minStock: request.MinStock,
            categoryId: request.CategoryId,
            supplierId: request.SupplierId
        );

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Products.GetByIdAsync(product.Id);
        return MapToDto(updated!);
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        var product = await _uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException("Producto", id);

        if (product.IsActive)
            product.Deactivate();
        else
            product.Activate();

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();
    }

    // ─── Mapeo ────────────────────────────────────────────────────────────────

    private static ProductResponse MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        SKU = product.SKU,
        PurchasePrice = product.PurchasePrice,
        SalePrice = product.SalePrice,
        Margin = product.Margin,
        MarginPercent = product.GetMarginPercent(),
        Stock = product.Stock,
        MinStock = product.MinStock,
        IsActive = product.IsActive,
        IsLowStock = product.IsLowStock(),
        IsOutOfStock = product.IsOutOfStock(),
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.Name ?? string.Empty,
        SupplierId = product.SupplierId,
        SupplierName = product.Supplier?.Name,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}