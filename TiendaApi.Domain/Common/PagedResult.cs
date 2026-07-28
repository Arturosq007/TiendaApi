namespace TiendaApi.Domain.Common;
public class PagedResult<T>(IReadOnlyCollection<T> items, int totalCount, int page, int pageSize)
{
    public IReadOnlyCollection<T> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    /// Crea un resultado vacío — útil cuando no hay datos.
    public static PagedResult<T> Empty(int page, int pageSize)
        => new([], 0, page, pageSize);
}