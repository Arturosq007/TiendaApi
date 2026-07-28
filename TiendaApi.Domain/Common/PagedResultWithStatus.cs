namespace TiendaApi.Domain.Common;
public class PagedResultWithStatus<T>(
    IReadOnlyCollection<T> items,
    int totalCount,
    int page,
    int pageSize,
    int activesCount,
    int inactivesCount) : PagedResult<T>(items, totalCount, page, pageSize)
{
    public int ActivesCount { get; } = activesCount;
    public int InactivesCount { get; } = inactivesCount;

    public static new PagedResultWithStatus<T> Empty(int page, int pageSize)
        => new([], 0, page, pageSize, 0, 0);
}