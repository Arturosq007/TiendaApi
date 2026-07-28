namespace TiendaApi.Domain.Common;
public abstract class ActivatableEntity : BaseEntity
{
    public bool IsActive { get; set; } = true;
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}   