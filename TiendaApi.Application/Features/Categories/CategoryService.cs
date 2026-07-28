using TiendaApi.Application.Features.Categories.Request;
using TiendaApi.Application.Features.Categories.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Categories;

public class CategoryService(IUnitOfWork _uow) : ICategoryService
{
    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var existingCategory = await _uow.Categories.NameExistsAsync(request.Name);
        if (existingCategory)
            throw new DomainException("Ya existe una categoría con el mismo nombre.");

        var category = Category.Create(request.Name, request.Description);
        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Categoría", id);
        return MapToDto(category);
    }

    public async Task<PagedResultWithStatus<CategoryResponse>> GetPagedAsync(int page, int pageSize, string? search = null, bool? isActive = null)
    {
        var result = await _uow.Categories.GetPagedAsync(page, pageSize, search, isActive);
        var dtos = result.Items.Select(MapToDto).ToList();

        return new PagedResultWithStatus<CategoryResponse>(  
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
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Categoría", id);

        if (category.IsActive)
        {
            var hasProducts = await _uow.Categories.HasProductsAsync(id);
            if (hasProducts)
                throw new DomainException("No se puede desactivar la categoría porque tiene productos asociados.");
            category.Deactivate();
        }
        else
            category.Activate();
        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException("Categoría", id);
        var existingCategory = await _uow.Categories.NameExistsAsync(request.Name, id);
        if (existingCategory)
            throw new DomainException("Ya existe una categoría con el mismo nombre.");
        category.Update(request.Name, request.Description);
        
        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();

        return MapToDto(category);
    }

    private static CategoryResponse MapToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IsActive = category.IsActive,
        CreatedAt = category.CreatedAt,
        UpdatedAt = category.UpdatedAt,
    };
}