using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;

public class Category : ActivatableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ICollection<Product> Products { get; private set; } = [];
    private Category() { }
    private Category(string name, string? description)
    {
        Name = ValidateName(name);
        Description = description;
    }
    public static Category Create(string name, string? description = null)
    {
        return new Category(name, description);
    }
    public void Update(string newName, string? newDescription)
    {
        Name = ValidateName(newName);
        Description = newDescription?.Trim();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la categoría es obligatorio.");

        if (name.Trim().Length > 100)
            throw new DomainException("El nombre no puede superar los 100 caracteres.");

        return name.Trim();
    }
}
