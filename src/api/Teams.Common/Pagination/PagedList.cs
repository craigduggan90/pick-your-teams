namespace Teams.Common.Pagination;

public record PagedList<T>(IReadOnlyCollection<T> Data, string? Cursor, int Count);