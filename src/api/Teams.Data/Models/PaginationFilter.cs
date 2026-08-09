namespace Teams.Data.Models;

/// <summary>Model containing filter values used when paging base entity queries.</summary>
/// <param name="Cursor">The offset cursor; default 0.</param>
/// <param name="PageSize">The maximum number of results to return; default 25.</param>
public record PaginationFilter(long? Cursor = null, int? PageSize = null);