namespace TiendaApi.Application.Features.Categories.Request;

public interface ICategoryRequest
{
    string Name { get; }
    string? Description { get; }
}

// 2. Hacemos que tus records implementen la interfaz
public record CreateCategoryRequest(string Name, string? Description) : ICategoryRequest;
public record UpdateCategoryRequest(string Name, string? Description) : ICategoryRequest;